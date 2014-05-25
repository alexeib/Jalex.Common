﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Jalex.Infrastructure.Repository;
using Jalex.Infrastructure.Utils;

namespace Jalex.Repository.Utils
{
    internal class ReflectedTypeDescriptor<T> : IReflectedTypeDescriptor<T>
    {
        protected readonly Type _type;
        protected readonly Action<T, string> _idSetter;
        protected readonly Func<T, string> _idGetter;

        public string TypeName { get { return _type.Name; } }
        public bool IsIdAutoGenerated { get; private set; }
        public string IdPropertyName { get; private set; }
        public PropertyInfo[] Properties { get; private set; }

        public Expression<Func<T, string>> IdGetterExpression { get; private set; }

        public ParameterExpression TypeParameter { get; private set; }
        public MemberExpression IdPropertyExpression { get; private set; }

        public ReflectedTypeDescriptor()
        {
            _type = typeof (T);

            Properties = _type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            bool isIdAutoGeneratedLocal;
            IdPropertyName = getIdPropertyName(Properties, out isIdAutoGeneratedLocal);
            IsIdAutoGenerated = isIdAutoGeneratedLocal;       

            IdGetterExpression = ExpressionProperties.GetPropertyGetterExpression<T, string>(IdPropertyName);
            _idGetter = IdGetterExpression.Compile();
            _idSetter = ExpressionProperties.GetPropertySetter<T, string>(IdPropertyName);

            TypeParameter = Expression.Parameter(_type);
            IdPropertyExpression = Expression.Property(TypeParameter, IdPropertyName);
        }

        public string GetId(T target)
        {
            ParameterChecker.CheckForNull(target, "target");
            return _idGetter(target);
        }

        public void SetId(T target, string id)
        {
            _idSetter(target, id);
        }

        private static string getIdPropertyName(PropertyInfo[] classProps, out bool isAutoGenerated)
        {
            PropertyInfo idProperty;

            // try to get id propert through attribute annotation first
            var idPropertyAndAttribute = (from prop in classProps
                                          let idAttribute = (IdAttribute)prop.GetCustomAttributes(true).FirstOrDefault(a => a is IdAttribute)
                                          where idAttribute != null
                                          select new { Property = prop, IdAttribute = idAttribute }).FirstOrDefault();

            if (idPropertyAndAttribute != null)
            {
                idProperty = idPropertyAndAttribute.Property;
                isAutoGenerated = idPropertyAndAttribute.IdAttribute.IsAutoGenerated;
            }
            else
            {
                // if no attribute is present, try using convention
                idProperty = classProps.FirstOrDefault(m => RepositoryConstants.IdFieldNames.Contains(m.Name));
                isAutoGenerated = true;
            }

            if (idProperty == null)
            {
                throw new RepositoryException("Id property not found (must be one of " +
                                              string.Join(", ", RepositoryConstants.IdFieldNames) +
                                              "). Alternatively, set a IdAttribute on the key property.");
            }

            string idPropertyName = idProperty.Name;

            if (idProperty.PropertyType != typeof(string))
            {
                throw new RepositoryException("Id property " + idPropertyName + " must be of type string");
            }
            return idPropertyName;
        }
    }
}
