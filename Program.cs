using Microsoft.Data.SqlClient;
using ConsolePojectForPizzaMizzaDB.DataBase;

public class Program
{
    public static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.InputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("===pizza*mizza===\n");

        while (true)
        {
            Console.WriteLine("1. Pizzalar");
            Console.WriteLine("2. İstifadəçinin xüsusi seçimlərinə əsasən pizza");
            Console.WriteLine("0. Çıxış etmək\n");
            Console.Write("Seçiminiz: ");
            string choice = Console.ReadLine();
            Console.WriteLine();

            switch (choice)
            {
                case "1":
                    ShowAllPizzas();
                    Console.WriteLine();
                    break;
                case "2":
                    CreateCustomPizza();
                    Console.WriteLine();
                    break;
                case "0":
                    Console.WriteLine("Proqramdan çıxış edilir...");
                    return;
                default:
                    Console.WriteLine("Sadəcə göstərilən seçimlər edilə bilər\n");
                    break;
            }
        }
    }

    public static void ShowAllPizzas()
    {
        // Consolda 1 daxil ediləndə bütün pizzalar ekrana çıxmalıdı.
        //Pizzanın Id -si,
        //Pizzanın adı,
        //Tipi,
        //Qiyməti.

        using (SqlConnection conn = DataBase.GetConnection())
        {
            conn.Open();

            string cmdText = @"SELECT 
                                    P.Id, 
                                    P.Name, 
                                    PT.TypeName, 
                                    PP.Price 
                                FROM Pizzas AS P
                                INNER JOIN PizzaTypes AS PT ON P.TypeId = PT.Id
                                INNER JOIN PizzaPrices AS PP ON P.Id = PP.PizzaId
                                ORDER BY P.Id, P.Name, PT.TypeName, PP.Price";

            SqlCommand cmd = new SqlCommand(cmdText, conn);
            SqlDataReader reader = cmd.ExecuteReader();

            var pizzas = new Dictionary<string, List<string>>();

            while (reader.Read())
            {
                string key = $"{reader["Id"]} | {reader["Name"]} | {reader["TypeName"]}";
                string price = reader["Price"].ToString();

                if (!pizzas.ContainsKey(key))
                    pizzas[key] = new List<string>();

                pizzas[key].Add(price);
            }

            foreach (var item in pizzas)
            {
                var parts = item.Key.Split('|');
                string id = parts[0].Trim();
                string name = parts[1].Trim();
                string type = parts[2].Trim();

                Console.WriteLine($"Pizza {id}");
                Console.WriteLine($"  Adı: {name}");
                Console.WriteLine($"  Tipi: {type}");
                Console.WriteLine($"  Qiymətlər: {string.Join(" | ", item.Value)} ₼\n");
            }

            Console.Write("Pizza haqqında ətraflı məlumat üçün ID daxil edin (çıxmaq üçün 0): ");
            string input = Console.ReadLine();
            Console.WriteLine();

            if (int.TryParse(input, out int pizzaId) && pizzaId > 0)
            {
                //Pizzaların siyahısı çıxandan sonra:
                //"Pizza haqqında ətraflı məlumat almaq istəyirsizsə pizzanın İd -sini ,istəmirsizə 0 daxil edin" - Mesajı çıxsın.
                //İd->daxil edildikdə həmin İd - yə uyğun pizzanın:
                //         İngredientləri
                //         Ardınca, hər ölçü üçün qiymətləri consola çıxsın
                //0->daxil edildikdə əsas menyuya geri dönsün
                ShowPizzaDetails(pizzaId);
            }
        }
    }
    public static void ShowPizzaDetails(int pizzaId)
    {

        using (SqlConnection conn = DataBase.GetConnection())
        {
            conn.Open();

            string cmdText = @"SELECT
                                    P.Id AS PizzaId,
                                    P.Name AS PizzaName,
                                    I.Name AS IngredientName,
                                    S.SizeName,
                                    PP.Price
                                FROM PizzaIngredients AS PI
                                INNER JOIN Ingredients AS I ON PI.IngredientId = I.Id
                                INNER JOIN Pizzas AS P ON PI.PizzaId = P.Id
                                INNER JOIN PizzaPrices AS PP ON P.Id = PP.PizzaId
                                INNER JOIN Sizes AS S ON S.Id = PP.SizeId
                                WHERE P.Id = @id
                                ORDER BY S.SizeName, I.Name";

            SqlCommand cmd = new SqlCommand(cmdText, conn);
            cmd.Parameters.AddWithValue("@id", pizzaId);
            SqlDataReader reader = cmd.ExecuteReader();

            bool hasRows = false;
            Console.WriteLine("-------------------------------------------------");
            while (reader.Read())
            {
                hasRows = true;
                Console.WriteLine($"Pizza ID: {reader["PizzaId"]}");
                Console.WriteLine($"Adı: {reader["PizzaName"]}");
                Console.WriteLine($"Ingredient: {reader["IngredientName"]}");
                Console.WriteLine($"Ölçü: {reader["SizeName"]}");
                Console.WriteLine($"Qiymət: {reader["Price"]} ₼");
                Console.WriteLine();
            }
            Console.WriteLine("-------------------------------------------------");
            if (!hasRows)
            {
                Console.WriteLine("Belə bir pizza tapılmadı.\n");
            }
        }
    }
    public static void CreateCustomPizza()
    {
        using (SqlConnection conn = DataBase.GetConnection())
        {
            conn.Open();

            using (SqlTransaction tran = conn.BeginTransaction())
            {
                try
                {
                    Console.Write("Pizzanın adını daxil edin: ");
                    string pizzaName = Console.ReadLine();

                    Console.WriteLine("Pizza tipləri: 1.Classic  2.Premium  3.Vegetarian  4.Special");
                    Console.Write("Tip seçin: ");
                    int typeId = int.Parse(Console.ReadLine());


                    Console.WriteLine("\nİngredientlər:");
                    Console.WriteLine("1.Pendir 2.Mozzarella 3.Soğan 4.Göbələk 5.Zeytun 6.Pomidor");
                    Console.WriteLine("7.Kolbasa 8.Toyuq 9.Vetçina 10.Mərci sousu 11.BBQ 12.Halapenyo");
                    Console.Write("İngredientləri seçin (məs: 1,3,6): ");
                    string ingredientInput = Console.ReadLine();
                    string[] ingredientIds = ingredientInput.Split(',');


                    Console.Write("Mini qiymət: ");
                    decimal priceMini = decimal.Parse(Console.ReadLine());

                    Console.Write("Midi qiymət: ");
                    decimal priceMidi = decimal.Parse(Console.ReadLine());

                    Console.Write("Maxi qiymət: ");
                    decimal priceMaxi = decimal.Parse(Console.ReadLine());

                    string insertPizza = @"
                    INSERT INTO Pizzas (Name, TypeId)
                    VALUES (@name, @typeId);
                    SELECT SCOPE_IDENTITY();
                ";

                    SqlCommand cmdInsertPizza = new SqlCommand(insertPizza, conn, tran);
                    cmdInsertPizza.Parameters.AddWithValue("@name", pizzaName);
                    cmdInsertPizza.Parameters.AddWithValue("@typeId", typeId);

                    int newPizzaId = Convert.ToInt32(cmdInsertPizza.ExecuteScalar());

                    string insertIngredient = @"
                    INSERT INTO PizzaIngredients (PizzaId, IngredientId)
                    VALUES (@pizzaId, @ingredientId);
                ";

                    foreach (var id in ingredientIds)
                    {
                        SqlCommand cmdIng = new SqlCommand(insertIngredient, conn, tran);
                        cmdIng.Parameters.AddWithValue("@pizzaId", newPizzaId);
                        cmdIng.Parameters.AddWithValue("@ingredientId", int.Parse(id.Trim()));
                        cmdIng.ExecuteNonQuery();
                    }

                    string insertPrice = @"
                    INSERT INTO PizzaPrices (PizzaId, SizeId, Price)
                    VALUES 
                        (@pizzaId, 1, @mini),
                        (@pizzaId, 2, @midi),
                        (@pizzaId, 3, @maxi);
                ";

                    SqlCommand cmdPrice = new SqlCommand(insertPrice, conn, tran);
                    cmdPrice.Parameters.AddWithValue("@pizzaId", newPizzaId);
                    cmdPrice.Parameters.AddWithValue("@mini", priceMini);
                    cmdPrice.Parameters.AddWithValue("@midi", priceMidi);
                    cmdPrice.Parameters.AddWithValue("@maxi", priceMaxi);
                    cmdPrice.ExecuteNonQuery();

                    tran.Commit();

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\nYeni pizza uğurla yaradıldı!");
                    Console.ResetColor();
                }
                catch (Exception ex)
                {
                    tran.Rollback();

                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\nXəta baş verdi: " + ex.Message);
                    Console.ResetColor();
                }
            }
        }
    }


}