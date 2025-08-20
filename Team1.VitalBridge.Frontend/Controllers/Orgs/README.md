# 機構查詢API文件

## 概述
此API專為VitalBridge前台提供機構查詢功能，支援多條件搜尋和詳細資料檢視。

## API基礎資訊
- **基礎URL**: `http://localhost:5232/api/Organizations`
- **內容類型**: `application/json`
- **命名空間**: `Team1.VitalBridge.Frontend.Controllers.Orgs`

## API端點

### 1. 機構搜尋
```
POST /api/Organizations/search232
```

#### 請求參數 (OrganizationSearchDto)
```json
{
  "keyword": "string?",           // 關鍵字搜尋 (機構名稱)
  "cityId": "int?",              // 縣市ID
  "districtId": "int?",          // 鄉鎮區ID  
  "organizationTypeIds": [1,2],  // 機構類型ID列表 (多選)
  "minPrice": "int?",            // 最低價格
  "maxPrice": "int?",            // 最高價格
  "featureServiceIds": [1,2,3],  // 特色服務ID列表 (多選)
  "page": 1,                     // 頁數 (預設1)
  "pageSize": 10                 // 每頁大小 (預設10)
}
```

#### 回應 (PaginatedResultDto<OrganizationListItemDto>)
```json
{
  "items": [
    {
      "id": 1,
      "name": "機構名稱",
      "cityName": "縣市名稱",
      "districtName": "鄉鎮區名稱",
      "address": "地址",
      "bedCount": 50,
      "photoUrl": "檔案名稱.jpg",
      "typeName": "機構類型",
      "minMonthlyPrice": 25000,
      "isRecommended": true,
      "isCertified": true,
      "featureServiceNames": ["復健治療", "營養師諮詢"]
    }
  ],
  "totalCount": 100,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 10,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

### 2. 機構詳細資料
```
GET /api/Organizations/{id}
```

#### 回應 (OrganizationDetailDto)
```json
{
  "id": 1,
  "name": "機構名稱",
  "cityName": "縣市名稱",
  "districtName": "鄉鎮區名稱",
  "address": "完整地址",
  "bedCount": 50,
  "photoUrl": "檔案名稱.jpg",
  "typeName": "機構類型",
  "description": "機構介紹",
  "mapUrl": "Google地圖連結",
  "ageLimits": "收容年齡限制",
  "isRecommended": true,
  "isCertified": true,
  "featureServices": [
    {
      "id": 1,
      "name": "復健治療",
      "imageUrl": "圖示檔名.png"
    }
  ],
  "serviceTargetNames": ["失智症患者", "中風患者"],
  "rooms": [
    {
      "id": 1,
      "roomTypeName": "單人房",
      "monthlyPrice": 30000,
      "roomQuantity": 10,
      "hasDeposit": true,
      "depositAmount": 60000,
      "depositMonths": 2.0
    }
  ],
  "subsidyInfoDescriptions": ["長照2.0補助", "身心障礙補助"]
}
```

### 3. 輔助查詢API

#### 縣市列表
```
GET /api/Organizations/cities
```

#### 鄉鎮區列表
```
GET /api/Organizations/cities/{cityId}/districts
```

#### 機構類型列表
```
GET /api/Organizations/organization-types
```

#### 特色服務列表
```
GET /api/Organizations/feature-services
```

## 圖片存取
所有圖片檔案都透過統一的檔案API存取：
```
/api/UploadFile/GetFile?fileName={檔案名稱}
```

例如：
- 機構照片：`/api/UploadFile/GetFile?fileName=org_photo.jpg`
- 特色服務圖示：`/api/UploadFile/GetFile?fileName=service_icon.png`

## 搜尋功能說明

### 關鍵字搜尋
- 支援機構名稱模糊搜尋
- 使用SQL LIKE查詢

### 價格範圍
- 基於機構房型的月費進行篩選
- 支援最低價格和最高價格區間查詢

### 多選篩選
- 機構類型：可同時選擇多種類型
- 特色服務：機構必須具備所選的特色服務

### 排序規則
1. 推薦機構優先 (IsRecommended)
2. 認證機構次之 (IsCertified)  
3. 機構名稱排序

## 分頁資訊
- 預設每頁10筆資料
- 支援自定義每頁大小
- 提供完整分頁資訊 (總筆數、總頁數、是否有上下頁等)

## 錯誤處理
- 統一回傳HTTP狀態碼
- 提供中文錯誤訊息
- 記錄詳細錯誤日誌

## 注意事項
- 只查詢啟用且未刪除的機構 (IsActive=true, IsDeleted=false)
- 只顯示啟用的機構類型和特色服務
- 所有查詢都使用AsNoTracking()提升效能