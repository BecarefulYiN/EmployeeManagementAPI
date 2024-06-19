using Microsoft.AspNetCore.Components.Server;

namespace EmployeeManagementAPI.Queries
{
    public class DepartmentQuery
    {
        public static string GetDepartmentListQuery()
        {
            return @"
            SELECT 
                d.[DepartmentId],
                d.[DepartmentName],
                d.[IsActive],
                COUNT(e.[EmployeeId]) AS EmployeeCount
            FROM 
                [dbo].[DepartmentTable] d
            LEFT JOIN 
                [dbo].[EmployeeTable] e ON d.[DepartmentId] = e.[DepartmentId]
            WHERE 
                d.[IsActive] = @IsActive
            GROUP BY 
                d.[DepartmentId], d.[DepartmentName], d.[IsActive]";
        }

        public static string GetDepartmentByIdQuery()
        {
            return @"
        SELECT 
            d.[DepartmentId],
            d.[DepartmentName],
            d.[IsActive],
            COUNT(e.[EmployeeId]) AS EmployeeCount
        FROM 
            [dbo].[DepartmentTable] d
        LEFT JOIN 
            [dbo].[EmployeeTable] e ON d.[DepartmentId] = e.[DepartmentId]
        WHERE 
            d.[DepartmentId] = @DepartmentId
        GROUP BY 
            d.[DepartmentId], d.[DepartmentName], d.[IsActive]";
        }

        public static string GetCheckDepartmentNameDuplicateQuery()
        {
            return @"SELECT [DepartmentId]
      ,[DepartmentName]
      ,[IsActive]
  FROM [dbo].[DepartmentTable] WHERE DepartmentName = @DepartmentName AND IsActive = @IsActive";
        }

        public static string InsertDepartmentQuery()
        {
            return @"INSERT INTO [dbo].[DepartmentTable]
           ([DepartmentName]
           ,[IsActive])
     VALUES (@DepartmentName, @IsActive)";
        }

        public static string GetCheckDepartmentExistsQuery()
        {
            return @"SELECT [DepartmentId]
      ,[DepartmentName]
      ,[IsActive]
  FROM [dbo].[DepartmentTable] WHERE DepartmentId = @DepartmentId";
        }

        public static string GetCheckDuplicateRoleQuery()
        {
            return @"SELECT COUNT(*) FROM [dbo].[DepartmentTable] WHERE DepartmentName = @DepartmentName AND DepartmentId != @DepartmentId";
        }

        public static string GetUpdateDepartmentQuery()
        {
            return @"UPDATE [dbo].[DepartmentTable] SET DepartmentName = @DepartmentName WHERE [DepartmentId] = @DepartmentId";
        }

        public static string GetDeleteDepartmentQuery()
        {
            return @"UPDATE [dbo].[DepartmentTable]
   SET IsActive = @IsActive
 WHERE DepartmentId = @DepartmentId";
        }
    }
}
