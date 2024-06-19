namespace EmployeeManagementAPI.Queries
{
    public class EmployeeProjectQuery
    {
        public static string GetEmployeeProjectListQuery()
        {
            return @"SELECT [EmployeeId]
      ,[ProjectId]
  FROM [dbo].[EmployeeProjectTable] 
            WHERE IsActive = @IsActive";
        }

        public static string GetCheckEmployeeExistsQuery()
        {
            return @"SELECT COUNT(*) FROM [dbo].[EmployeeTable] WHERE EmployeeId = @EmployeeId AND IsActive = @IsActive";
        }

        public static string GetCheckProjectExistsQuery()
        {
            return @"SELECT COUNT(*) FROM [dbo].[ProjectTable] WHERE ProjectId = @ProjectId AND IsActive = @IsActive";
        }

        public static string GetCheckDuplicateEmployeeProjectAssociation()
        {
            return @"SELECT [EmployeeId], [ProjectId], [IsActive]
                                      FROM [dbo].[EmployeeProjectTable]
                                      WHERE EmployeeId = @EmployeeId AND ProjectId = @ProjectId AND IsActive = @IsActive";
        }

        public static string GetInsertEmployeeProjectQuery()
        {
            return @"INSERT INTO [dbo].[EmployeeProjectTable]
                             ([EmployeeId], [ProjectId], [IsActive])
                             VALUES (@EmployeeId, @ProjectId, @IsActive)";
        }

        public static string GetDeleteEmployeeProjectQuery()
        {
            return @"UPDATE [dbo].[EmployeeProjectTable]
   SET IsActive = @IsActive
 WHERE ProjectId = @ProjectId AND EmployeeId = @EmployeeId";
        }
    }
}
