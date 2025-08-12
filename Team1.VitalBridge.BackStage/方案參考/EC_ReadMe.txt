[v] 設定EF Core
	-建立 EfModels
	-設定連接字串
	-註冊 AppDbContext 到 DI 容器
	-設定 _Layout 的 html_lang value 為 zh-tw
[todo] 實作 商品分類管理ProductCategory
	-採用前後端分離寫法：
	CategoryController → 只負責顯示表單畫面（符合原設計）
	CategoryApiController → 處理所有資料操作（符合原設計）
	[V]-add ProductCategoryFormViewModel 商品分類
	[V]目前已經先把顯示樹狀圖的部分，改為三層式架構
	[V]目前在寫Index 顯示類別採用fetch API 的方式，動態載入資料 ，顯示樹狀圖
	[V]新增商品類別畫面樣子已做，在Index.cshtml
	[V]PI Controller Create
	[V]新增分類 action & View Page
	[進行中- 預計要和]API Controller Update
	[todo]API Controller Delete
	-add 新增分類 action & View Page
	-add Service (判斷是否重複名稱、自己不能選自己、自己不能選自己子分類)
	-add Interface,Repository 實際CRUD對資料庫操作

[todo] 物流設定
	[v] 列表顯示
	[working] 新增物流方式
	[todo] 編輯物流方式


[todo] 實作 商品照ProductImage
	-add ProductImageViewModel
	-add ProductImageService (排序邏輯、驗證、CRUD Repository 的業務邏輯)
	-add Interface , Repository 實際CRUD對資料庫操作

	
[todo] 實作 商品運送方式Ships
	-add ShipController
	-add ShipViewModel
	-add ShipsService (驗證、CRUD Repository 的業務邏輯)
	-add Interface , Repository 實際CRUD對資料庫操作

[todo] 實作 商品Products
	-add ProductController
	-add ProductCreateViewModel (製作中) / ProductListViewModel
	-add ProductService (驗證、CRUD Repository 的業務邏輯)
	-add Interface , Repository 實際CRUD對資料庫操作