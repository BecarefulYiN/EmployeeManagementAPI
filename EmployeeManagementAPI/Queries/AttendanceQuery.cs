namespace EmployeeManagementAPI.Queries;

public class AttendanceQuery
{
    public static string GetCheckEmployeeQuery()
    {
        return @"SELECT [EmployeeId] FROM [dbo].[EmployeeTable] WHERE EmployeeId = @EmployeeId";
    }

    public static string InsertAttendanceQuery()
    {
        return @"INSERT INTO [dbo].[AttendanceTable]
                                  ([EmployeeId], [Date], [Status])
                                  VALUES
                                  (@EmployeeId, @Date, @Status)";
    }

    public static string GetAllAttendanceRecordsQuery()
    {
        return @"
            SELECT 
                A.AttendanceId, 
                E.FirstName + ' ' + E.LastName AS EmployeeName,
                E.Email,
                A.Date,
                A.Status
            FROM 
                [dbo].[AttendanceTable] A
            JOIN 
                [dbo].[EmployeeTable] E
            ON 
                A.EmployeeId = E.EmployeeId";
    }

    public static string GetCheckEmployeeExistQuery()
    {
        return @"SELECT COUNT(*) FROM [dbo].[EmployeeTable] WHERE [EmployeeId] = @EmployeeId";
    }

    public static string GetAttendanceRecordsByEmployee()
    {
        return @"SELECT [AttendanceId], [EmployeeId], [Date], [Status]
                          FROM [dbo].[AttendanceTable] WHERE [EmployeeId] = @EmployeeId";
    }
}
