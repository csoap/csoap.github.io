//---------------------------------------------------------------------------
//
// <copyright file="SystemDataExtensionMethods.cs" company="Microsoft">
//    Copyright (C) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// Description: Helper methods for code that uses types from System.Data.
//
//---------------------------------------------------------------------------

using System;
using System.ComponentModel;

namespace MS.Internal
{
    internal abstract class SystemDataExtensionMethods
    {
        // return true if the list is a DataView
        internal abstract bool IsDataView(IBindingList list);

        // return true if the item is a DataRowView
        internal abstract bool IsDataRowView(object item);

        // return true if the value is null in the SqlTypes sense
        internal abstract bool IsSqlNull(object value);

        // return true if the type is nullable in the SqlTypes sense
        internal abstract bool IsSqlNullableType(Type type);

        // ADO DataSet exposes some properties that cause problems involving
        // identity and change notifications.  We handle these specially.
        internal abstract bool IsDataSetCollectionProperty(PropertyDescriptor pd);

        // return true if DBNull is a valid value for the given item and column.
        // The column may be specified directly by name, or indirectly by indexer: Item[arg]
        internal abstract bool DetermineWhetherDBNullIsValid(object item, string columnName, object arg);

    }
}
