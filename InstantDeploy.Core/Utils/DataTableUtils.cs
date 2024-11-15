namespace InstantDeploy.Utils;

/// <summary>
/// DataTable 工具类
/// </summary>
public static class DataTableUtils
{
    /// <summary>
    /// DataTable 是否有数据行
    /// </summary>
    /// <param name="dt"> </param>
    /// <returns> </returns>
    public static bool HasRows(this DataTable dt) => dt.Rows.Count > 0;

    /// <summary>
    /// <see cref="List{T}" /> To DataTable
    /// </summary>
    /// <typeparam name="T"> </typeparam>
    /// <param name="list"> </param>
    /// <param name="tableName"> </param>
    /// <returns> </returns>
    public static DataTable ToDataTable<T>(this List<T> list, string? tableName = null)
    {
        tableName ??= "sheet";
        var targetSource = new DataTable(tableName);

        if (list.Count == 0)
        {
            foreach (var property in typeof(T).GetProperties())
            {
                var underlyingType = Nullable.GetUnderlyingType(property.PropertyType);
                if (underlyingType != null)
                {
                    // 如果该类型具有可为空性
                    var column = new DataColumn(property.Name, underlyingType);
                    column.AllowDBNull = true;
                    targetSource.Columns.Add(column);
                }
                else
                {
                    // 没有可为空性
                    var column = new DataColumn(property.Name, property.PropertyType);
                    targetSource.Columns.Add(column);
                }
            }
            return targetSource;
        }

        var properties = list[0]!.GetType().GetProperties();
        targetSource.Columns.AddRange(
            properties
                .Select(p =>
                {
                    if (
                        p.PropertyType.IsGenericType
                        && p.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>)
                    )
                    {
                        return new DataColumn(
                            p.GetCustomAttribute<DescriptionAttribute>()?.Description ?? p.Name,
                            Nullable.GetUnderlyingType(p.PropertyType)
                        );
                    }

                    return new DataColumn(
                        p.GetCustomAttribute<DescriptionAttribute>()?.Description ?? p.Name,
                        p.PropertyType
                    );
                })
                .ToArray()
        );

        list.ForEach(item =>
            targetSource.LoadDataRow(properties.Select(p => p.GetValue(item)).ToArray(), true)
        );

        return targetSource;
    }

    /// <summary>
    /// <see cref="DataRow[]" /> To DataTable
    /// </summary>
    /// <param name="rows"> </param>
    /// <returns> </returns>
    public static DataTable ToDataTable(this DataRow[] rows)
    {
        if (rows.Length <= 0)
            return new DataTable();

        var targetSource = rows[0].Table.Clone();
        targetSource.DefaultView.Sort = rows[0].Table.DefaultView.Sort;
        foreach (var t in rows)
        {
            targetSource.LoadDataRow(t.ItemArray, true);
        }

        return targetSource;
    }

    /// <summary>
    /// DataTable To <see cref="List{T}" />
    /// </summary>
    /// <typeparam name="T"> </typeparam>
    /// <param name="dataTable"> </param>
    /// <returns> </returns>
    public static List<T> ToList<T>(this DataTable dataTable)
        where T : class, new()
    {
        var targetSource = new List<T>();
        if (dataTable == null || dataTable.Rows.Count <= 0)
            return targetSource;

        targetSource.AddRange(
            dataTable
                .Rows.Cast<DataRow>()
                .Select(info => DataTableBuilder<T>.CreateBuilder(dataTable.Rows[0]).Build(info))
        );
        return targetSource;
    }
}

/// <summary>
/// DataTable Builder
/// </summary>
/// <typeparam name="T"> </typeparam>
internal sealed class DataTableBuilder<T>
{
    private static readonly MethodInfo GetValueMethod = typeof(DataRow).GetMethod(
        "get_Item",
        new[] { typeof(int) }
    );

    private static readonly MethodInfo IsDbNullMethod = typeof(DataRow).GetMethod(
        "IsNull",
        new[] { typeof(int) }
    );

    private Load _handler;

    private DataTableBuilder()
    { }

    private delegate T Load(DataRow dataRecord);

    public static DataTableBuilder<T> CreateBuilder(DataRow dataRecord)
    {
        var methodCreateEntity = new DynamicMethod(
            "DynamicCreateEntity",
            typeof(T),
            new[] { typeof(DataRow) },
            typeof(T),
            true
        );
        var generator = methodCreateEntity.GetILGenerator();
        var result = generator.DeclareLocal(typeof(T));
        generator.Emit(OpCodes.Newobj, typeof(T).GetConstructor(Type.EmptyTypes)!);
        generator.Emit(OpCodes.Stloc, result);
        for (int i = 0; i < dataRecord.ItemArray.Length; i++)
        {
            var propertyInfo = typeof(T).GetProperty(dataRecord.Table.Columns[i].ColumnName);
            var endIfLabel = generator.DefineLabel();
            if (propertyInfo == null || propertyInfo.GetSetMethod() == null)
            {
                continue;
            }

            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldc_I4, i);
            generator.Emit(OpCodes.Callvirt, IsDbNullMethod);
            generator.Emit(OpCodes.Brtrue, endIfLabel);
            generator.Emit(OpCodes.Ldloc, result);
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldc_I4, i);
            generator.Emit(OpCodes.Callvirt, GetValueMethod);
            generator.Emit(OpCodes.Unbox_Any, propertyInfo.PropertyType);
            generator.Emit(OpCodes.Callvirt, propertyInfo.GetSetMethod()!);
            generator.MarkLabel(endIfLabel);
        }

        generator.Emit(OpCodes.Ldloc, result);
        generator.Emit(OpCodes.Ret);
        return new DataTableBuilder<T>
        {
            _handler = (Load)methodCreateEntity.CreateDelegate(typeof(Load))
        };
    }

    public T Build(DataRow dataRecord) => _handler(dataRecord);
}