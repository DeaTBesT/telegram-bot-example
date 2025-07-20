using System.Data;
using System.Diagnostics;
using Dapper;
using FantasyKingdom.Enums;
using FantasyKingdom.Models;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using SQLitePCL;

namespace FantasyKingdom.Services;

public static class DatabaseService
{
    private const string dbFile = "./database.db";

    private static IDbConnection _connection;

    public static async void Run()
    {
        Logger.Log($"Подключение к базе данных по пути: {Path.GetFullPath(dbFile)}");
        
        if (!File.Exists(dbFile))
        {
            Logger.LogError("Database not found");
        }

        _connection = new SqliteConnection($"Data Source={dbFile};");
        raw.SetProvider(new SQLite3Provider_e_sqlite3());
        
        Logger.Log("Подключение к базе данных успешно установлено");
    }

    public static List<UserModel> GetAllUsers()
    {
        // Получаем всех пользователей из базы данных
        var dbUsers = _connection.Query<DataBaseUserModel>("SELECT Id, UserName, Data FROM Users").ToList();

        var userModels = new List<UserModel>();

        foreach (var dbUser in dbUsers)
        {
            try
            {
                var userData = JsonConvert.DeserializeObject<UserData>(dbUser.Data);

                userModels.Add(new UserModel
                {
                    Id = dbUser.Id,
                    UserName = dbUser.UserName,
                    Data = userData
                });
            }
            catch (JsonException ex)
            {
                Logger.LogWarning($"Ошибка десериализации пользовательских данных для ID пользователя {dbUser.Id}: {ex.Message}");
            }
        }

        return userModels;
    }
    
    public static UserModel GetUserById(long id)
    {
        var output = _connection.Query<DataBaseUserModel>($"SELECT UserName, Data FROM Users WHERE Id='{id}'").FirstOrDefault();

        if (output == null)
        {
            return null;
        }
        
        var userData = JsonConvert.DeserializeObject<UserData>(output.Data);

        var userModel = new UserModel
        {
            Id = id,
            UserName = output.UserName,
            Data = userData
        };
        
        return userModel;
    }

    public static bool TrySaveUser(UserModel user)
    {
        if (!IsUserHas(user.Id))
        {
            return false;
        }
        
        var data = JsonConvert.SerializeObject(user.Data);

        _connection.Execute(
            "UPDATE Users SET UserName = @UserName, Data = @data WHERE Id = @Id",
            new
            {
                user.UserName,
                data,
                user.Id
            }
        );

        return true;
    }

    public static bool TryCreateUser(long id)
    {
        Logger.Log(id.ToString());
        
        if (IsUserHas(id))
        {
            return false;
        }

        var userData = new UserData
        {
            UserState = UserAction.registration,
            TavernInfo = new UserData.TavernPanel(),
            //InventoryInfo = new UserData.UserInventory()
        };

        var user = new UserModel(id, id.ToString(), userData);
        var data = JsonConvert.SerializeObject(user.Data);

        _connection.Execute("INSERT INTO Users (Id, UserName, Data) VALUES (@Id, @UserName, @data)",
            new { user.Id, user.UserName, data });


        return true;
    }

    public static bool IsUserHas(long id) =>
        _connection.Query<string>($"SELECT * FROM Users WHERE Id='{id}'").Any();
}