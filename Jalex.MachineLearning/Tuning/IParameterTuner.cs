using System;
using System.Collections.Generic;

namespace Jalex.MachineLearning.Tuning
{
    public interface IParameterTuner
    {
        TParameters FindOptimal<TParameters, TResult>(TParameters initialParameters,
                                                      Func<IEnumerable<TParameters>, Func<TParameters, string>, IEnumerable<Tuple<TParameters, TResult>>> run,
                                                      Func<TResult, TResult, bool> isLeftBetterThanRight,
                                                      IEnumerable<string> includedProps = null,
                                                      IEnumerable<string> excludedProps = null) where TParameters : class where TResult : class;
    }
}