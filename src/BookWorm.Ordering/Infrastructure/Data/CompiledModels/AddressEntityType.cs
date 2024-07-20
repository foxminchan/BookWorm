﻿// <auto-generated />
using System;
using System.Reflection;
using BookWorm.Ordering.Domain.BuyerAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

#pragma warning disable 219, 612, 618
#nullable disable

namespace BookWorm.Ordering.Infrastructure.Data.CompiledModels
{
    internal partial class AddressEntityType
    {
        public static RuntimeEntityType Create(RuntimeModel model, RuntimeEntityType baseEntityType = null)
        {
            var runtimeEntityType = model.AddEntityType(
                "BookWorm.Ordering.Domain.BuyerAggregate.Address",
                typeof(Address),
                baseEntityType);

            var buyerId = runtimeEntityType.AddProperty(
                "BuyerId",
                typeof(Guid),
                valueGenerated: ValueGenerated.OnAdd,
                afterSaveBehavior: PropertySaveBehavior.Throw,
                sentinel: new Guid("00000000-0000-0000-0000-000000000000"));
            buyerId.TypeMapping = GuidTypeMapping.Default.Clone(
                comparer: new ValueComparer<Guid>(
                    (Guid v1, Guid v2) => v1 == v2,
                    (Guid v) => v.GetHashCode(),
                    (Guid v) => v),
                keyComparer: new ValueComparer<Guid>(
                    (Guid v1, Guid v2) => v1 == v2,
                    (Guid v) => v.GetHashCode(),
                    (Guid v) => v),
                providerValueComparer: new ValueComparer<Guid>(
                    (Guid v1, Guid v2) => v1 == v2,
                    (Guid v) => v.GetHashCode(),
                    (Guid v) => v),
                mappingInfo: new RelationalTypeMappingInfo(
                    storeTypeName: "uuid"));
            buyerId.AddAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.None);
            buyerId.AddAnnotation("Relational:ColumnName", "id");

            var city = runtimeEntityType.AddProperty(
                "City",
                typeof(string),
                propertyInfo: typeof(Address).GetProperty("City", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(Address).GetField("<City>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                nullable: true,
                maxLength: 50);
            city.TypeMapping = NpgsqlStringTypeMapping.Default.Clone(
                comparer: new ValueComparer<string>(
                    (string v1, string v2) => v1 == v2,
                    (string v) => v.GetHashCode(),
                    (string v) => v),
                keyComparer: new ValueComparer<string>(
                    (string v1, string v2) => v1 == v2,
                    (string v) => v.GetHashCode(),
                    (string v) => v),
                providerValueComparer: new ValueComparer<string>(
                    (string v1, string v2) => v1 == v2,
                    (string v) => v.GetHashCode(),
                    (string v) => v),
                mappingInfo: new RelationalTypeMappingInfo(
                    storeTypeName: "character varying(50)",
                    size: 50));
            city.TypeMapping = ((NpgsqlStringTypeMapping)city.TypeMapping).Clone(npgsqlDbType: NpgsqlTypes.NpgsqlDbType.Varchar);
        city.AddAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.None);
        city.AddAnnotation("Relational:ColumnName", "address_city");

        var province = runtimeEntityType.AddProperty(
            "Province",
            typeof(string),
            propertyInfo: typeof(Address).GetProperty("Province", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
            fieldInfo: typeof(Address).GetField("<Province>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
            nullable: true,
            maxLength: 50);
        province.TypeMapping = NpgsqlStringTypeMapping.Default.Clone(
            comparer: new ValueComparer<string>(
                (string v1, string v2) => v1 == v2,
                (string v) => v.GetHashCode(),
                (string v) => v),
            keyComparer: new ValueComparer<string>(
                (string v1, string v2) => v1 == v2,
                (string v) => v.GetHashCode(),
                (string v) => v),
            providerValueComparer: new ValueComparer<string>(
                (string v1, string v2) => v1 == v2,
                (string v) => v.GetHashCode(),
                (string v) => v),
            mappingInfo: new RelationalTypeMappingInfo(
                storeTypeName: "character varying(50)",
                size: 50));
        province.TypeMapping = ((NpgsqlStringTypeMapping)province.TypeMapping).Clone(npgsqlDbType: NpgsqlTypes.NpgsqlDbType.Varchar);
    province.AddAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.None);
    province.AddAnnotation("Relational:ColumnName", "address_province");

    var street = runtimeEntityType.AddProperty(
        "Street",
        typeof(string),
        propertyInfo: typeof(Address).GetProperty("Street", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
        fieldInfo: typeof(Address).GetField("<Street>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
        nullable: true,
        maxLength: 50);
    street.TypeMapping = NpgsqlStringTypeMapping.Default.Clone(
        comparer: new ValueComparer<string>(
            (string v1, string v2) => v1 == v2,
            (string v) => v.GetHashCode(),
            (string v) => v),
        keyComparer: new ValueComparer<string>(
            (string v1, string v2) => v1 == v2,
            (string v) => v.GetHashCode(),
            (string v) => v),
        providerValueComparer: new ValueComparer<string>(
            (string v1, string v2) => v1 == v2,
            (string v) => v.GetHashCode(),
            (string v) => v),
        mappingInfo: new RelationalTypeMappingInfo(
            storeTypeName: "character varying(50)",
            size: 50));
    street.TypeMapping = ((NpgsqlStringTypeMapping)street.TypeMapping).Clone(npgsqlDbType: NpgsqlTypes.NpgsqlDbType.Varchar);
street.AddAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.None);
street.AddAnnotation("Relational:ColumnName", "address_street");

var key = runtimeEntityType.AddKey(
    new[] { buyerId });
runtimeEntityType.SetPrimaryKey(key);

return runtimeEntityType;
}

public static RuntimeForeignKey CreateForeignKey1(RuntimeEntityType declaringEntityType, RuntimeEntityType principalEntityType)
{
    var runtimeForeignKey = declaringEntityType.AddForeignKey(new[] { declaringEntityType.FindProperty("BuyerId") },
        principalEntityType.FindKey(new[] { principalEntityType.FindProperty("Id") }),
        principalEntityType,
        deleteBehavior: DeleteBehavior.Cascade,
        unique: true,
        required: true,
        ownership: true);

    var address = principalEntityType.AddNavigation("Address",
        runtimeForeignKey,
        onDependent: false,
        typeof(Address),
        propertyInfo: typeof(Buyer).GetProperty("Address", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
        fieldInfo: typeof(Buyer).GetField("<Address>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
        propertyAccessMode: PropertyAccessMode.Property,
        eagerLoaded: true);

    runtimeForeignKey.AddAnnotation("Relational:Name", "fk_buyers_buyers_id");
    return runtimeForeignKey;
}

public static void CreateAnnotations(RuntimeEntityType runtimeEntityType)
{
    runtimeEntityType.AddAnnotation("Relational:FunctionName", null);
    runtimeEntityType.AddAnnotation("Relational:Schema", null);
    runtimeEntityType.AddAnnotation("Relational:SqlQuery", null);
    runtimeEntityType.AddAnnotation("Relational:TableName", "buyers");
    runtimeEntityType.AddAnnotation("Relational:ViewName", null);
    runtimeEntityType.AddAnnotation("Relational:ViewSchema", null);

    Customize(runtimeEntityType);
}

static partial void Customize(RuntimeEntityType runtimeEntityType);
}
}
