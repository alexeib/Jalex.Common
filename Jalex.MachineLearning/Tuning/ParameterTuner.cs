using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Jalex.Infrastructure.Extensions;
using Jalex.MachineLearning.Tuning.Tuners;
using NLog;

namespace Jalex.MachineLearning.Tuning
{
    public class ParameterTuner : IParameterTuner
    {
        private const int _maxAttempts = 3;

        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        private readonly IValueMoverFactory _valueMoverFactory;

        public ParameterTuner(IValueMoverFactory valueMoverFactoryFactory)
        {
            if (valueMoverFactoryFactory == null) throw new ArgumentNullException(nameof(valueMoverFactoryFactory));

            _valueMoverFactory = valueMoverFactoryFactory;
        }

        public TParameters FindOptimal<TParameters, TResult>(TParameters initialParameters,
                                                             Func<IEnumerable<TParameters>, Func<TParameters, string>, IEnumerable<Tuple<TParameters, TResult>>> run,
                                                             Func<TResult, TResult, bool> isLeftBetterThanRight,
                                                             IEnumerable<string> includedProps = null,
                                                             IEnumerable<string> excludedProps = null)
            where TParameters : class where TResult : class
        {
            if (initialParameters == null) throw new ArgumentNullException(nameof(initialParameters));
            if (run == null) throw new ArgumentNullException(nameof(run));
            if (isLeftBetterThanRight == null) throw new ArgumentNullException(nameof(isLeftBetterThanRight));

            var propsToTune = getAllPropsToTune(initialParameters, includedProps, excludedProps);
            var tunedProps = new HashSet<PropertyMover>();

            _logger.Info("Beginning to tune properties:" + propsToTune.Aggregate(string.Empty, (acc, next) => acc + $"\n\t{next.Name}"));

            Random random = new Random();

            TParameters currentOptimalParameters = initialParameters;

            while (propsToTune.Count > 0)
            {
                _logger.Info($"{propsToTune.Count} properties left to tune");

                var propToTuneIdx = random.Next(propsToTune.Count);
                var propMover = propsToTune[propToTuneIdx];

                int attempts = 0;

                while (attempts++ < _maxAttempts && propsToTune.Contains(propMover))
                {
                    _logger.Info($"Tuning property {propMover.Name}");

                    var candidates = propMover.GetMovedParameters(currentOptimalParameters)
                                              .ToCollection();
                    if (candidates.Count == 0)
                    {
                        propsToTune.Remove(propMover);
                        continue;
                    }

                    var paramsToRun = currentOptimalParameters.ToEnumerable()
                                                              .Concat(candidates);
                    var results = run(paramsToRun, p => $"{propMover.Name} = {propMover.GetValue(p)}");

                    TParameters optParams = null;
                    TResult optResult = null;
                    bool failedToComputeAllResults = false;

                    foreach (var result in results)
                    {
                        if (result.Item2 == null)
                        {
                            failedToComputeAllResults = true;
                            break;
                        }
                        if (optResult == null || isLeftBetterThanRight(result.Item2, optResult))
                        {
                            optParams = result.Item1;
                            optResult = result.Item2;
                        }
                    }

                    if (optParams == null || failedToComputeAllResults)
                    {
                        // no or subset of results retrieved, maybe bad date - continue
                        _logger.Info("Skipping this run as results were not computed for all moves");
                        continue;
                    }

                    if (Equals(optParams, currentOptimalParameters))
                    {
                        _logger.Info($"Finished {propMover.Name}. Current optimal parameters {propMover.Name} = {propMover.GetValue(optParams)} are still optimal.");

                        if (!tunedProps.Add(propMover))
                        {
                            _logger.Info($"Removing {propMover.Name}");
                            propsToTune.Remove(propMover);
                        }
                    }
                    else
                    {
                        _logger.Info($"Finished {propMover.Name} and obtained new optimal parameters with {propMover.Name} = {propMover.GetValue(optParams)}");
                        _logger.Info("New optimal parameters:");
                        _logger.Info(optParams.ToString());
                        _logger.Info("New optimal result:");
                        _logger.Info(optResult?.ToString());
                        _logger.Info("\n");

                        currentOptimalParameters = optParams;

                        tunedProps.Remove(propMover);
                    }
                }
            }

            return currentOptimalParameters;
        }

        private IList<PropertyMover> getAllPropsToTune(object parameters, IEnumerable<string> includedProps, IEnumerable<string> excludedProps)
        {
            var tunableProps = getAllTunableProps(parameters, x => x);
            if (includedProps != null)
            {
                var includedPropsSet = includedProps.ToHashSet();
                tunableProps = tunableProps.Where(t => includedPropsSet.Contains(t.Name));
            }
            if (excludedProps != null)
            {
                var excludedPropsSet = excludedProps.ToHashSet();
                tunableProps = tunableProps.Where(t => !excludedPropsSet.Contains(t.Name));
            }

            return tunableProps.ToList();
        }

        private IEnumerable<PropertyMover> getAllTunableProps(object parameters, Func<object, object> getCurrentObjFromInitial, string prefix = "")
        {
            var type = parameters.GetType();
            var props = type.GetProperties();

            foreach (var prop in props)
            {
                if (!prop.CanWrite) continue;

                var tuningOptions = prop.GetCustomAttribute<TunableParameter>();
                if (tuningOptions == null)
                {
                    continue;
                }

                var valueMover = _valueMoverFactory.CreateValueMover(prop);
                if (valueMover != null)
                {
                    yield return createValueMover(getCurrentObjFromInitial, prefix, string.Empty, prop, valueMover);
                }
                else if (prop.PropertyType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(prop.PropertyType))
                {
                    foreach (var p in getPropertyMoversForList(parameters, prop, getCurrentObjFromInitial, prefix)) yield return p;
                }
                else
                {
                    var val = prop.GetValue(parameters);
                    if (val != null)
                    {
                        foreach (var p in getAllTunableProps(val, x => prop.GetValue(x), prefix + prop.Name + ".")) yield return p;
                    }
                }
            }
        }

        private IEnumerable<PropertyMover> getPropertyMoversForList(object parameters, PropertyInfo prop, Func<object, object> getCurrentObjFromInitial, string prefix)
        {
            var values = (IList)prop.GetValue(parameters);
            if (values != null)
            {
                int idx = 0;
                foreach (var val in values)
                {
                    if (val == null) continue;

                    var indexedPropName = prefix + prop.Name + "[" + idx + "]";
                    var localIdx = idx;

                    Func<object, object> getIndexedObj = obj =>
                                                         {
                                                             var currObj = getCurrentObjFromInitial(obj);
                                                             var collection = (IList)prop.GetValue(currObj);
                                                             var itemAtIndex = collection[localIdx];
                                                             return itemAtIndex;
                                                         };

                    var valueMover = _valueMoverFactory.CreateValueMover(prop);
                    if (valueMover != null)
                    {
                        yield return new PropertyMover(indexedPropName,
                                                       valueMover,
                                                       getIndexedObj,
                                                       (obj, value) =>
                                                       {
                                                           var currObj = getCurrentObjFromInitial(obj);
                                                           var collection = (IList)prop.GetValue(currObj);
                                                           collection[localIdx] = value;
                                                       },
                                                       obj => obj.ToJson()
                                                                 .FromJson(obj.GetType()));
                    }
                    else
                    {
                        foreach (var nestedProp in getAllTunableProps(val, getIndexedObj, indexedPropName + "."))
                        {
                            yield return nestedProp;
                        }
                    }
                    idx++;
                }
            }
        }

        private static PropertyMover createValueMover(Func<object, object> getCurrentObjFromInitial, string prefix, string postfix, PropertyInfo prop, IValueMover valueMover)
        {
            return new PropertyMover(prefix + prop.Name + postfix,
                                     valueMover,
                                     obj =>
                                     {
                                         var currentObj = getCurrentObjFromInitial(obj);
                                         return prop.GetValue(currentObj);
                                     },
                                     (obj, value) =>
                                     {
                                         var currentObj = getCurrentObjFromInitial(obj);
                                         prop.SetValue(currentObj, value);
                                     },
                                     obj => obj.ToJson()
                                               .FromJson(obj.GetType()));
        }
    }
}
