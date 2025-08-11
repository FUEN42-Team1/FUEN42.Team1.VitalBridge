namespace Team1.VitalBridge.BackStage.Models.DataTables
{
    public class DataTableRequest
    {
        public int Draw { get; set; } // 用來同步前後端的請求次數
        public int Start { get; set; } // 起始索引（分頁用）
        public int Length { get; set; } // 每頁筆數

        public Search Search { get; set; } // 全域搜尋條件
        public List<Order> Order { get; set; } // 排序設定
        public List<Column> Columns { get; set; } // 欄位資訊
    }

    public class Search
    {
        public string Value { get; set; } // 搜尋關鍵字
        public bool Regex { get; set; } // 是否使用正則表達式
    }

    public class Order
    {
        public int Column { get; set; } // 欄位索引
        public string Dir { get; set; } // 排序方向（asc 或 desc）
    }

    public class Column
    {
        public string Data { get; set; } // 欄位名稱
        public string Name { get; set; } // 顯示名稱
        public bool Searchable { get; set; } // 是否可搜尋
        public bool Orderable { get; set; } // 是否可排序
        public Search Search { get; set; } // 欄位搜尋條件
    }

    public class DataTableResponse<T>
    {
        public int Draw { get; set; }
        public int RecordsTotal { get; set; }
        public int RecordsFiltered { get; set; }
        public List<T> Data { get; set; }
    }
}
