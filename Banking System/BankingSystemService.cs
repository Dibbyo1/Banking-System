using System;
using System.Data.SqlClient;

public class BankingSystemService
{
    private readonly DatabaseService _db = new DatabaseService();

    public int ShowMainMenu()
    {
        Console.WriteLine("==== Banking System ====");
        Console.WriteLine("1. Login");
        Console.WriteLine("2. Open New Account");
        Console.WriteLine("3. About Us");
        Console.WriteLine("4. Exit");
        Console.Write("Enter your choice: ");

        if (int.TryParse(Console.ReadLine(), out int choice))
            return choice;
        return 0;
    }

    public void SignUp()
    {
        Console.Clear();
        Console.WriteLine("-- Sign Up --");

        Console.Write("Full Name: ");
        _db.Name = Console.ReadLine();

        Console.Write("Gender (M/F): ");
        char gender = Console.ReadLine().ToUpper()[0];
        _db.Title = gender == 'M' ? "Mr" : "Ms";

        Console.Write("Password (max 21 chars): ");
        string plainPassword = Console.ReadLine();
        _db.Password = _db.HashPassword(plainPassword);

        Console.Write("Initial Deposit: ");
        if (!double.TryParse(Console.ReadLine(), out double deposit))
        {
            Console.WriteLine("Invalid amount.");
            return;
        }
        _db.TotalBalance = deposit;

        _db.AccountNumber = _db.GenerateAccountNumber();
        _db.WriteToDatabase(2);
        _db.UpdatePassbook(deposit, "Initial Deposit");

        Console.WriteLine($"\nAccount created successfully! Your account number is {_db.AccountNumber}");
    }

    public void Login()
    {
        Console.Clear();
        Console.WriteLine("-- Login --");

        Console.Write("Account Number: ");
        if (!uint.TryParse(Console.ReadLine(), out uint accNum))
        {
            Console.WriteLine("Invalid account number.");
            return;
        }
        _db.AccountNumber = accNum;

        if (!_db.ReadFromDatabase())
        {
            Console.WriteLine("Account not found.");
            return;
        }

        Console.Write("Password: ");
        string inputPwd = Console.ReadLine();
        string hashedInputPwd = _db.HashPassword(inputPwd);

        if (_db.Password != hashedInputPwd)
        {
            Console.WriteLine("Incorrect password.");
            return;
        }

        LoggedInMenu();
    }

    private void LoggedInMenu()
    {
        bool isLoggedIn = true;
        while (isLoggedIn)
        {
            Console.Clear();
            Console.WriteLine($"-- Welcome {_db.Title}. {_db.Name} --");
            Console.WriteLine("1. Deposit Money");
            Console.WriteLine("2. Withdraw Money");
            Console.WriteLine("3. Transfer Money");
            Console.WriteLine("4. View Passbook");
            Console.WriteLine("5. Account Details");
            Console.WriteLine("6. Logout");
            Console.Write("Choose an option: ");

            if (!int.TryParse(Console.ReadLine(), out int choice))
                continue;

            switch (choice)
            {
                case 1: Deposit(); break;
                case 2: Withdraw(); break;
                case 3: Transfer(); break;
                case 4: ViewPassbook(); break;
                case 5: ShowDetails(); break;
                case 6:
                    Console.WriteLine("Logging out...");
                    isLoggedIn = false;
                    break;
                default: Console.WriteLine("Invalid option."); break;
            }

            if (isLoggedIn)
            {
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
            }
        }
    }

    private void Deposit()
    {
        Console.Write("Enter amount to deposit: ");
        if (double.TryParse(Console.ReadLine(), out double amount))
        {
            _db.TotalBalance += amount;
            _db.UpdatePassbook(amount, "Deposit");
            _db.WriteToDatabase(4);
            Console.WriteLine($"Rs. {amount} deposited. New Balance: {_db.TotalBalance}");
        }
        else Console.WriteLine("Invalid input.");
    }

    private void Withdraw()
    {
        Console.Write("Enter amount to withdraw: ");
        if (double.TryParse(Console.ReadLine(), out double amount))
        {
            if (amount > _db.TotalBalance)
                Console.WriteLine("Insufficient balance.");
            else
            {
                _db.TotalBalance -= amount;
                _db.UpdatePassbook(amount, "Withdrawal");
                _db.WriteToDatabase(4);
                Console.WriteLine($"Rs. {amount} withdrawn. Remaining Balance: {_db.TotalBalance}");
            }
        }
        else Console.WriteLine("Invalid input.");
    }

    private void Transfer()
    {
        Console.Write("Enter recipient account number: ");
        if (!uint.TryParse(Console.ReadLine(), out uint recipientAcc))
        {
            Console.WriteLine("Invalid account number.");
            return;
        }

        Console.Write("Enter amount to transfer: ");
        if (!double.TryParse(Console.ReadLine(), out double amount) || amount > _db.TotalBalance)
        {
            Console.WriteLine("Invalid amount or insufficient funds.");
            return;
        }

        var recipientDb = new DatabaseService { AccountNumber = recipientAcc };
        if (!recipientDb.ReadFromDatabase())
        {
            Console.WriteLine("Recipient account not found.");
            return;
        }

        _db.TotalBalance -= amount;
        recipientDb.TotalBalance += amount;

        _db.WriteToDatabase(4);
        recipientDb.WriteToDatabase(4);

        _db.UpdatePassbook(amount, $"Transfer to {recipientAcc}");
        recipientDb.UpdatePassbook(amount, $"Transfer from {_db.AccountNumber}");

        Console.WriteLine($"Rs. {amount} transferred to {recipientDb.Name}.");
    }

    private void ShowDetails()
    {
        Console.WriteLine($"Account Number: {_db.AccountNumber}");
        Console.WriteLine($"Name: {_db.Title}. {_db.Name}");
        Console.WriteLine($"Balance: Rs. {_db.TotalBalance}");
        Console.WriteLine($"Last Login: {_db.LastLoginDetails}");
    }

    private void ViewPassbook()
    {
        using SqlDataReader reader = _db.ReadPassbook();
        Console.WriteLine("\n-- Passbook --");
        Console.WriteLine("Amount\t\tDateTime\t\t\tDescription");
        while (reader.Read())
        {
            Console.WriteLine($"{reader[1]}\t{reader[2]}\t{reader[3]}");
        }
    }

    public void ShowAbout()
    {
        Console.WriteLine("Banking System");
        Console.WriteLine("Developed by Dibbyo Saha");
    }
}