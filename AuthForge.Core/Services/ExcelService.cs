using AuthForge.Core.Models;
using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Text;

namespace AuthForge.Core.Services
{
    public class ExcelService
    {
        // Создание шаблона для пользователя
        public void CreateTemplate(string filePath)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Users");
            worksheet.Cell(1, 1).Value = "Login";
            worksheet.Cell(1, 2).Value = "Password";
            worksheet.Columns().AdjustToContents();
            workbook.SaveAs(filePath);
        }

        // Чтение данных из файла
        public List<UserInput> ReadEmployees(string filePath)
        {
            var users = new List<UserInput>();
            using var workbook = new XLWorkbook(filePath);
            var worksheet = workbook.Worksheet(1);
            var rows = worksheet.RangeUsed().RowsUsed().Skip(1); // Пропускаем заголовок

            foreach (var row in rows)
            {
                users.Add(new UserInput(
                    row.Cell(1).GetString(),
                    row.Cell(2).GetString()
                ));
            }
            return users;
        }

        // Сохранение результата
        public void SaveResults(string filePath, List<UserOutput> results)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Hashed Data");

            // Заголовки
            worksheet.Cell(1, 1).Value = "Login";
            worksheet.Cell(1, 2).Value = "Password Hash";
            worksheet.Cell(1, 3).Value = "Salt";
            worksheet.Cell(1, 4).Value = "Algorithm";

            // Данные
            for (int i = 0; i < results.Count; i++)
            {
                worksheet.Cell(i + 2, 1).Value = results[i].Login;
                worksheet.Cell(i + 2, 2).Value = results[i].PasswordHash;
                worksheet.Cell(i + 2, 3).Value = results[i].Salt;
                worksheet.Cell(i + 2, 4).Value = results[i].Algorithm;
            }

            worksheet.Columns().AdjustToContents();
            workbook.SaveAs(filePath);
        }

        public bool IsTemplateValid(string filePath)
        {
            try
            {
                using (var workbook = new XLWorkbook(filePath))
                {
                    var worksheet = workbook.Worksheet(1);
                    // Получаем значения ячеек A1 и B1
                    string col1 = worksheet.Cell(1, 1).GetString().Trim();
                    string col2 = worksheet.Cell(1, 2).GetString().Trim();

                    return col1.Equals("Login", StringComparison.OrdinalIgnoreCase) &&
                           col2.Equals("Password", StringComparison.OrdinalIgnoreCase);
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
