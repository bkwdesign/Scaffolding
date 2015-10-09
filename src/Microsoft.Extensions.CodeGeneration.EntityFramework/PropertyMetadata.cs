// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Extensions.CodeGeneration.EntityFramework
{
    public class PropertyMetadata
    {
        public PropertyMetadata(IProperty property, Type dbContextType)
        {
            Contract.Assert(property != null);
            Contract.Assert(property.ClrType != null); //Do we need to make sure this is not called on Shadow properties?

            PropertyName = property.Name;
            TypeName = property.ClrType.FullName;

            IsPrimaryKey = property.IsPrimaryKey();
            // The old scaffolding has some logic for this property in an edge case which is
            // not clear if needed any more; see EntityFrameworkColumnProvider.DetermineIsForeignKeyComponent
            IsForeignKey = property.IsForeignKey(property.DeclaringEntityType);

            IsEnum = property.ClrType.GetTypeInfo().IsEnum;
            IsReadOnly = property.IsReadOnlyBeforeSave;
            IsAutoGenerated = property.StoreGeneratedAlways;

            // Todo:we need proper logic for these below.
            ShortTypeName = property.ClrType.FullName;

            Scaffold = true;
            var reflectionProperty = property.ClrType.GetProperty(property.Name);
            if (reflectionProperty != null)
            {
                var scaffoldAttr = reflectionProperty.GetCustomAttribute(typeof(ScaffoldColumnAttribute)) as ScaffoldColumnAttribute;
                if (scaffoldAttr != null)
                {
                    Scaffold = scaffoldAttr.Scaffold;
                }
            }

            IsEnumFlags = false;
            if (IsEnum)
            {
                var flagsAttr = property.ClrType.GetTypeInfo().GetCustomAttribute(typeof(FlagsAttribute)) as FlagsAttribute;
                IsEnumFlags = (flagsAttr != null);
            }
        }

        public bool IsAutoGenerated { get; set; }

        public bool IsEnum { get; set; }

        public bool IsEnumFlags { get; set; }

        public bool IsForeignKey { get; set; }

        public bool IsPrimaryKey { get; set; }

        public bool IsReadOnly { get; set; }

        public string PropertyName { get; set; }

        public bool Scaffold { get; set; }

        public string ShortTypeName { get; set; }

        public string TypeName { get; set; }
    }
}