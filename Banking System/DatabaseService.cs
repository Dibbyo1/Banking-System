using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

public class DatabaseService
{
    private SqlConnection Connection;
    public UInt32 AccountNumber;
    public double TotalBalance;
    public string Title, Name, LastLoginDetails, Password;

    public DatabaseService()
    {
        string connectionString = ConfigurationManager.ConnectionStrings["BankDb"].ConnectionString;
        Connection = new SqlConnection(connectionString);
        try { Connection.Open(); }
        catch (SqlException ex)
        {
            Console.WriteLine("Error connecting to database: " + ex.Message);
            Environment.Exit(1);
        }
    }

    public string HashPassword(string plainPassword)
    {
        using SHA256 sha = SHA256.Create();
        byte[] bytes = Encoding.UTF8.GetBytes(plainPassword);
        byte[] hash = sha.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    private string GetQuery(int type)
    {
        return type switch
        {
            0 => "SELECT MAX(AccountNumber) FROM UserData",
            1 => $"SELECT * FROM UserData WHERE AccountNumber = {AccountNumber}",
            2 => $"INSERT INTO UserData VALUES({AccountNumber}, '{Title}', '{Name}', {TotalBalance}, '{Password}', SYSDATETIME())",
            3 => $"UPDATE UserData SET Title = '{Title}', Name = '{Name}', TotalBalance = {TotalBalance}, LastLoginDetails = SYSDATETIME() WHERE AccountNumber = {AccountNumber}",
            4 => $"UPDATE UserData SET TotalBalance = {TotalBalance} WHERE AccountNumber = {AccountNumber}",
            5 => $"SELECT * FROM Passbook WHERE AccountNumber = {AccountNumber}",
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public UInt32 GenerateAccountNumber()
    {
        SqlCommand cmd = new SqlCommand(GetQuery(0), Connection);
        SqlDataReader reader = cmd.ExecuteReader();
        reader.Read();
        AccountNumber = reader.IsDBNull(0) ? 12081999 : Convert.ToUInt32(reader.GetValue(0)) + 1;
        reader.Close();
        return AccountNumber;
    }

    public void WriteToDatabase(int queryType)
    {
        SqlCommand cmd = new SqlCommand(GetQuery(queryType), Connection);
        cmd.ExecuteNonQuery();
    }

    public bool ReadFromDatabase()
    {
        SqlCommand cmd = new SqlCommand(GetQuery(1), Connection);
        SqlDataReader reader = cmd.ExecuteReader();
        bool found = reader.Read();

        if (found)
        {
            Title = reader.GetString(1);
            Name = reader.GetString(2);
            TotalBalance = reader.GetDouble(3);
            Password = reader.GetString(4);
            LastLoginDetails = reader.GetDateTime(5).ToString();
        }

        reader.Close();
        return found;
    }

    public SqlDataReader ReadPassbook()
    {
        SqlCommand cmd = new SqlCommand(GetQuery(5), Connection);
        return cmd.ExecuteReader();
    }

    public void UpdatePassbook(double amount, string description)
    {
        string query = $"INSERT INTO Passbook VALUES({AccountNumber}, {amount}, SYSDATETIME(), '{description}')";
        SqlCommand cmd = new SqlCommand(query, Connection);
        cmd.ExecuteNonQuery();
    }

    public void Close()
    {
        Connection.Close();
        Connection.Dispose();
    }
}
