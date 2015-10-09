// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Extensions.CodeGeneration.EntityFramework
{
    // ToDo: This takes depedency on EF, that will conflict with
    // app's EF dependency
    public class ModelMetadata
    {
        private PropertyMetadata[] _properties;
        private PropertyMetadata[] _primaryKeys;
        private NavigationMetadata[] _navigations;

        //Todo: Perhaps move the constructor to something line MetadataReader?
        public ModelMetadata([NotNull]IEntityType entityType, [NotNull]Type dbContextType)
        {
            EntityType = entityType;
            DbContexType = dbContextType;
            EntitySetName = GetEntitySetName(DbContexType, EntityType.ClrType);
        }

        public IEntityType EntityType { get; private set; }

        public Type DbContexType { get; private set; }

        public string EntitySetName { get; private set; }

        public PropertyMetadata[] Properties
        {
            get
            {
                if (_properties == null)
                {
                    _properties = EntityType.GetProperties()
                        .Select(p => new PropertyMetadata(p, DbContexType))
                        .ToArray();
                }
                return _properties;
            }
        }

        public PropertyMetadata[] PrimaryKeys
        {
            get
            {
                if (_primaryKeys == null)
                {
                    _primaryKeys = EntityType.GetPrimaryKey()
                        .Properties
                        .Select(p => new PropertyMetadata(p, DbContexType))
                        .ToArray();
                }
                return _primaryKeys;
            }
        }

        public NavigationMetadata[] Navigations
        {
            get
            {
                if (_navigations == null)
                {
                    _navigations = EntityType.GetNavigations()
                        .Where(n => n.PointsToPrincipal() == true && n.ForeignKey.Properties.All(p => !p.IsShadowProperty))
                        .Select(n => new NavigationMetadata(n, DbContexType))
                        .ToArray();
                }
                return _navigations;
            }
        }

        internal static string GetEntitySetName([NotNull]Type dbContextType, [NotNull]Type modelType)
        {
            Type dbSetType = typeof(DbSet<>).MakeGenericType(modelType);

            var prop = dbContextType.GetRuntimeProperties()
                .Where(pi => pi.PropertyType == dbSetType)
                .FirstOrDefault();

            if (prop != null)
            {
                return prop.Name;
            }
            else
            {
                //Fallback to this or throw?
                return "Set<" + modelType.Name + ">()";
            }
        }
    }
}