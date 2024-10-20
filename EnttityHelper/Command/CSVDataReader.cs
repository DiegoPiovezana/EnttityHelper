using System;
using System.Data;
using System.IO;

namespace EH.Command
{
    internal class CSVDataReader : IDataReader
    {
        private StreamReader _reader;
        private string[]? _currentRow;
        private bool _isClosed;

        internal CSVDataReader(string filePath)
        {
            _reader = new StreamReader(filePath);
            _isClosed = false;
        }

        public bool Read()
        {
            if (_reader.EndOfStream) return false;
            var line = _reader.ReadLine();
            _currentRow = line.Split(',');
            return true;
        }

        public object GetValue(int i)
        {
            return _currentRow[i];
        }

        public int FieldCount => _currentRow?.Length ?? 0;

        // Implementar outros métodos da interface IDataReader conforme necessário
        // Exemplo de Close, GetFieldType, GetName, etc.

        public void Close()
        {
            _reader.Close();
            _isClosed = true;
        }

        public DataTable GetSchemaTable()
        {
            throw new NotImplementedException();
        }

        public bool NextResult()
        {
            throw new NotImplementedException();
        }

        public bool GetBoolean(int i)
        {
            throw new NotImplementedException();
        }

        public byte GetByte(int i)
        {
            throw new NotImplementedException();
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public char GetChar(int i)
        {
            throw new NotImplementedException();
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public IDataReader GetData(int i)
        {
            throw new NotImplementedException();
        }

        public string GetDataTypeName(int i)
        {
            throw new NotImplementedException();
        }

        public DateTime GetDateTime(int i)
        {
            throw new NotImplementedException();
        }

        public decimal GetDecimal(int i)
        {
            throw new NotImplementedException();
        }

        public double GetDouble(int i)
        {
            throw new NotImplementedException();
        }

        public Type GetFieldType(int i)
        {
            throw new NotImplementedException();
        }

        public float GetFloat(int i)
        {
            throw new NotImplementedException();
        }

        public Guid GetGuid(int i)
        {
            throw new NotImplementedException();
        }

        public short GetInt16(int i)
        {
            throw new NotImplementedException();
        }

        public int GetInt32(int i)
        {
            throw new NotImplementedException();
        }

        public long GetInt64(int i)
        {
            throw new NotImplementedException();
        }

        public string GetName(int i)
        {
            throw new NotImplementedException();
        }

        public int GetOrdinal(string name)
        {
            throw new NotImplementedException();
        }

        public string GetString(int i)
        {
            throw new NotImplementedException();
        }

        public int GetValues(object[] values)
        {
            throw new NotImplementedException();
        }

        public bool IsDBNull(int i)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public bool IsClosed => _isClosed;

        public int Depth => throw new NotImplementedException();

        public int RecordsAffected => throw new NotImplementedException();

        public object this[string name] => throw new NotImplementedException();

        public object this[int i] => throw new NotImplementedException();

        // Outros métodos da interface retornam NotImplementedException, pois são irrelevantes para converter para IDataReader




    }

}
