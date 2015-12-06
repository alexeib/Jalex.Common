﻿using System;
using System.Collections.Generic;
using Jalex.Infrastructure.ReflectedTypeDescriptor;
using Jalex.Infrastructure.Repository;
using Jalex.Repository.IdProviders;
using Magnum;
using NLog;

namespace Jalex.Repository
{
    public abstract class BaseRepository<T>
    {
        // ReSharper disable once StaticFieldInGenericType
        protected static readonly ILogger _staticLogger = LogManager.GetCurrentClassLogger();
        private ILogger _instanceLogger;

        protected readonly IReflectedTypeDescriptor<T> _typeDescriptor;
        protected readonly IIdProvider _idProvider;

        public ILogger Logger
        {
            get { return _instanceLogger ?? _staticLogger; }
            set { _instanceLogger = value; }
        }

        protected BaseRepository(
            IIdProvider idProvider,
            IReflectedTypeDescriptorProvider typeDescriptorProvider)
        {
            Guard.AgainstNull(idProvider, "idProvider");
            Guard.AgainstNull(typeDescriptorProvider, "typeDescriptorProvider");

            _idProvider = idProvider;
            _typeDescriptor = typeDescriptorProvider.GetReflectedTypeDescriptor<T>();
        }

        protected void ensureObjectIds(WriteMode writeMode, IEnumerable<T> objects)
        {
            HashSet<Guid> ids = new HashSet<Guid>();

            foreach (var obj in objects)
            {
                Guid id = _typeDescriptor.GetId(obj);

                if (id == Guid.Empty && writeMode == WriteMode.Update)
                {
                    throw new InvalidOperationException("Cannot update entity with empty id");
                }

                if (_typeDescriptor.IsIdAutoGenerated)
                {
                    id = checkOrGenerateIdForEntity(id, obj);
                }

                // skip dupe check if there are clustered indices
                if (!_typeDescriptor.HasClusteredIndices && !ids.Add(id))
                {
                    throw new DuplicateIdException("Attempting to create multiple objects with id " + id + " is not allowed");
                }
            }
        }

        private Guid checkOrGenerateIdForEntity(Guid id, T newObj)
        {
            if (id == Guid.Empty)
            {
                Guid generatedId = _idProvider.GenerateNewId();
                _typeDescriptor.SetId(newObj, generatedId);
                id = generatedId;
            }
            else if (!_idProvider.IsIdValid(id))
            {
                throw new IdFormatException(id + " is not a valid identifier (validated using " + _idProvider.GetType().Name + ")");
            }

            return id;
        }
    }
}
