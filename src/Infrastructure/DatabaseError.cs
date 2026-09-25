using Microsoft.Data.SqlClient;

namespace ElectronicsShop.Infrastructure
{
    public class DatabaseError
    {
        public static string GetMessage(Exception ex)
        {
            Exception error = ex.GetBaseException();
            SqlException? sqlError = error as SqlException;

            if (sqlError != null)
            {
                if (sqlError.Number == 2601 || sqlError.Number == 2627)
                    return "Запись с таким названием уже существует";

                if (sqlError.Number == 547)
                    return "Проверьте данные. Запись связана с другими записями или нарушает ограничения базы";

                if (sqlError.Number == 208)
                    return "Таблицы не найдены!";

                return "Не удалось подключиться к базе или выполнить запрос. Проверьте сервер и строку подключения.";
            }

            return error.Message;
        }
    }
}
