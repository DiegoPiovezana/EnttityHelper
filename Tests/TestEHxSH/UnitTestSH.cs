using EH;
//using SH;
using System.Data;

namespace TestEHxSH
{
    public class UnitTestSH
    {
        //[Fact]
        public void TestInsertIDataReader()
        {
            //EnttityHelper eh = new($"Data Source=172.26.8.159:1521/xe;User Id=system;Password=oracle");
            //SheetHelper sh = new();

            //if (eh.DbContext.ValidateConnection())
            //{
            //    const string origin = @"C:\Users\diego\Desktop\Tests\Converter\Small.xlsx";
            //    const string sheet = "1";

            //    var shReader = sh.GetSheetReader(origin, sheet);
            //    Assert.NotNull(shReader);

            //    //var dt = shReader.Reader.GetSchemaTable(); // NotSupportedException
            //    //DataTable dtResult = new();
            //    //dtResult.Load(shReader.Reader); // NotSupportedException GetSchemaTable
            //    //var dt3 = GetFirstRows(shReader.Reader, 5); // NotSupportedException GetName
            //    eh.Insert(shReader.Reader, null, true, "TB_TEST_IDATAREADER", true, 100); // NotSupportedException GetName

            //    shReader.Dispose();
            //}
        }

        DataTable GetFirstRows(IDataReader reader, int rowCount)
        {
            DataTable dataTable = new();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                dataTable.Columns.Add(reader.GetName(i), reader.GetFieldType(i));
            }

            for (int count = 0; reader.Read() && count < rowCount; count++)
            {
                DataRow row = dataTable.NewRow();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row[i] = reader[i];
                }
                dataTable.Rows.Add(row);
            }

            return dataTable;
        }

    }
}