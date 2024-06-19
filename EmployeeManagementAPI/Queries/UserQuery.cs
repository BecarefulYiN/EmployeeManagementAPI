namespace EmployeeManagementAPI.Queries
{
    public class UserQuery
    {
        public static string LoginQuery()
        {
            return @"
                    SELECT 
                        e.Email, 
                        r.RoleName 
                    FROM 
                        EmployeeTable e
                    JOIN 
                        EmployeeRoleTable r 
                    ON 
                        e.RoleId = r.RoleId 
                    WHERE 
                        e.Email = @Email";
        }
    }
}
