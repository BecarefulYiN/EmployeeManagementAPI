namespace EmployeeManagementAPI.Queries
{
    public class EmployeeRoleQuery
    {
        public static string GetEmployeeRoleListQuery()
        {
            return @"SELECT [RoleId]
      ,[RoleName]
      ,[IsActive]
  FROM [dbo].[EmployeeRoleTable] WHERE IsActive = @IsActive";
        }

        public static string GetDuplicateEmployeeQuery()
        {
            return @"SELECT [RoleId]
      ,[RoleName]
      ,[IsActive]
  FROM [dbo].[EmployeeRoleTable] WHERE RoleName = @RoleName";
        }

        public static string GetInsertEmployeeQuery()
        {
            return @"INSERT INTO [dbo].[EmployeeRoleTable]
           ([RoleName]
           ,[IsActive])
     VALUES
            (@RoleName, @IsActive);
            SELECT SCOPE_IDENTITY();";
        }

        public static string GetCheckEmployeeExistQuery()
        {
            return @"SELECT [RoleId], [RoleName], [IsActive] FROM [dbo].[EmployeeRoleTable] WHERE RoleId = @RoleId";
        }

        public static string GetDuplicateRoleQuery()
        {
            return @"SELECT COUNT(*) FROM [dbo].[EmployeeRoleTable] WHERE RoleName = @RoleName AND RoleId != @RoleId";
        }

        public static string GetUpdateEmployeeQuery()
        {
            return @"UPDATE [dbo].[EmployeeRoleTable] SET RoleName = @RoleName WHERE [RoleId] = @RoleId";
        }

        public static string GetDeleteEmployeeQuery()
        {
            return @"UPDATE [dbo].[EmployeeRoleTable]
   SET IsActive = @IsActive
 WHERE RoleId = @RoleId";
        }
    }
}
