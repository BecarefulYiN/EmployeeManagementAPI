namespace EmployeeManagementAPI.Queries
{
    public class EmployeeQuery
    {
        public static string GetEmployeeListQuery()
        {
            return @"
            SELECT e.[EmployeeId]
                  ,e.[FirstName]
                  ,e.[LastName]
                  ,e.[Email]
                  ,e.[PhoneNumber]
                  ,e.[HireDate]
                  ,d.[DepartmentName] AS DepartmentName
                  ,r.[RoleName] AS RoleName
                  ,e.[IsActive]
            FROM [dbo].[EmployeeTable] e
            LEFT JOIN [dbo].[DepartmentTable] d ON e.[DepartmentId] = d.[DepartmentId]
            LEFT JOIN [dbo].[EmployeeRoleTable] r ON e.[RoleId] = r.[RoleId]
            WHERE e.[IsActive] = @IsActive";
        }

        public static string GetEmployeeByIdQuery()
        {
            return @"
        SELECT e.[EmployeeId]
              ,e.[FirstName]
              ,e.[LastName]
              ,e.[Email]
              ,e.[PhoneNumber]
              ,e.[HireDate]
              ,d.[DepartmentName] AS DepartmentName
              ,r.[RoleName] AS RoleName
              ,e.[IsActive]
        FROM [dbo].[EmployeeTable] e
        LEFT JOIN [dbo].[DepartmentTable] d ON e.[DepartmentId] = d.[DepartmentId]
        LEFT JOIN [dbo].[EmployeeRoleTable] r ON e.[RoleId] = r.[RoleId]
        WHERE e.[EmployeeId] = @EmployeeId";
        }

        public static string GetCheckEmployeeEmailDuplicateQuery()
        {
            return @"SELECT [EmployeeId] FROM [dbo].[EmployeeTable] WHERE Email = @Email";
        }

        public static string GetCheckEmployeeRoleQuery()
        {
            return @"SELECT [RoleId] FROM [dbo].[EmployeeRoleTable] WHERE [RoleName] = @RoleName AND IsActive = @IsActive";
        }

        public static string GetInsertRoleQuery()
        {
            return @"INSERT INTO [dbo].[EmployeeRoleTable] ([RoleName], [IsActive]) VALUES (@RoleName, @IsActive);
                                           SELECT SCOPE_IDENTITY();";
        }

        public static string GetDepartmentQuery()
        {
            return @"SELECT [DepartmentId] FROM [dbo].[DepartmentTable] WHERE [DepartmentName] = @DepartmentName AND IsActive = @IsActive";
        }

        public static string InsertDepartmentQuery()
        {
            return @"INSERT INTO [dbo].[DepartmentTable] ([DepartmentName], [IsActive]) VALUES (@DepartmentName, @IsActive);
                                                 SELECT SCOPE_IDENTITY();";
        }

        public static string InsertEmployeeQuery()
        {
            return @"INSERT INTO [dbo].[EmployeeTable]
                                ([FirstName], [LastName], [Email], [PhoneNumber], [HireDate], [DepartmentId], [RoleId], [IsActive])
                                VALUES
                                (@FirstName, @LastName, @Email, @PhoneNumber, @HireDate, @DepartmentId, @RoleId, @IsActive);
                                SELECT SCOPE_IDENTITY();";
        }

        public static string GetCheckEmployeeExistsQuery()
        {
            return @"SELECT [EmployeeId], [FirstName], [LastName], [Email], [PhoneNumber], [HireDate], [DepartmentId], [RoleId], [IsActive]
                                   FROM [dbo].[EmployeeTable] 
                                   WHERE EmployeeId = @EmployeeId";
        }

        public static string GetCheckDuplicateEmailQuery()
        {
            return @"SELECT COUNT(*) FROM [dbo].[EmployeeTable] 
                                       WHERE [Email] = @Email AND [EmployeeId] != @EmployeeId";
        }

        public static string GetDepartmentIdByDepartmentNameQuery()
        {
            return @"SELECT [DepartmentId] 
                                       FROM [dbo].[DepartmentTable] 
                                       WHERE [DepartmentName] = @DepartmentName AND [IsActive] = 1";
        }

        public static string GetRoleIdByRoleName()
        {
            return @"SELECT [RoleId] 
                                 FROM [dbo].[EmployeeRoleTable] 
                                 WHERE [RoleName] = @RoleName AND [IsActive] = 1";
        }

        public static string GetUpdateEmployeeQuery()
        {
            return @"UPDATE [dbo].[EmployeeTable] SET
                               [FirstName] = @FirstName,
                               [LastName] = @LastName,
                               [Email] = @Email,
                               [PhoneNumber] = @PhoneNumber,
                               [DepartmentId] = @DepartmentId,
                               [RoleId] = @RoleId
                               WHERE [EmployeeId] = @EmployeeId";
        }

        public static string GetDeleteEmployeeQuery()
        {
            return @"UPDATE [dbo].[EmployeeTable]
   SET IsActive = @IsActive
 WHERE EmployeeId = @EmployeeId";
        }
    }
}
