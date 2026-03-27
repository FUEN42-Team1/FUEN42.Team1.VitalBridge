/**
 * 商品搜尋功能的 JavaScript 實作
 * 包含 AJAX 搜尋、分頁、篩選等功能
 */

// 全域變數
let currentSearchParams = {
    keyword: '',
    status: 'all',
    page: 1,
    pageSize: 10
};

/**
 * 頁面初始化
 */
$(document).ready(function () {
    console.log('商品搜尋頁面初始化開始');

    // 初始化頁面
    initializePage();

    // 綁定所有事件
    bindAllEvents();

    // 載入初始資料
    if (window.initialData) {
        updateProductTable(window.initialData);
        updatePagination(window.initialData);
        updateDataInfo(window.initialData);
    } else {
        // 如果沒有初始資料，執行搜尋
        loadProducts();
    }

    console.log('商品搜尋頁面初始化完成');
});

/**
 * 頁面初始化設定
 */
function initializePage() {
    // 設定預設的每頁筆數
    $('#pageSizeSelect').val(currentSearchParams.pageSize);

    // 設定預設的狀態篩選
    $('#statusSelect').val(currentSearchParams.status);
}

/**
 * 綁定所有事件處理器
 */
function bindAllEvents() {
    // 搜尋按鈕點擊事件
    $('#searchBtn').on('click', handleSearch);

    // 關鍵字輸入框按 Enter 鍵事件
    $('#keywordInput').on('keypress', function (e) {
        if (e.which === 13) { // Enter 鍵
            handleSearch();
        }
    });

    // 狀態篩選下拉選單變更事件
    $('#statusSelect').on('change', handleStatusChange);

    // 每頁筆數下拉選單變更事件
    $('#pageSizeSelect').on('change', handlePageSizeChange);

    // 分頁按鈕點擊事件（使用事件委派）
    $(document).on('click', '.page-link[data-page]', handlePageClick);
}

/**
 * 處理搜尋按鈕點擊事件
 */
function handleSearch() {
    console.log('執行搜尋');

    // 更新搜尋參數
    currentSearchParams.keyword = $('#keywordInput').val().trim();
    currentSearchParams.page = 1; // 重置到第一頁

    // 執行搜尋
    loadProducts();
}

/**
 * 處理狀態篩選變更事件
 */
function handleStatusChange() {
    console.log('狀態篩選變更');

    // 更新搜尋參數
    currentSearchParams.status = $('#statusSelect').val();
    currentSearchParams.page = 1; // 重置到第一頁

    // 執行搜尋
    loadProducts();
}

/**
 * 處理每頁筆數變更事件
 */
function handlePageSizeChange() {
    console.log('每頁筆數變更');

    // 更新搜尋參數
    currentSearchParams.pageSize = parseInt($('#pageSizeSelect').val());
    currentSearchParams.page = 1; // 重置到第一頁

    // 執行搜尋
    loadProducts();
}

/**
 * 處理分頁按鈕點擊事件
 */
function handlePageClick(e) {
    e.preventDefault();

    const page = parseInt($(this).data('page'));
    console.log('分頁點擊：', page);

    if (page && page !== currentSearchParams.page) {
        currentSearchParams.page = page;
        loadProducts();
    }
}

/**
 * AJAX 載入商品資料
 */
function loadProducts() {
    console.log('載入商品資料', currentSearchParams);

    // 顯示載入狀態（可選）
    $('#productTableBody').html('<tr><td colspan="7" class="text-center py-3">載入中...</td></tr>');

    // 發送 AJAX 請求
    $.ajax({
        url: '/Products/SearchProducts',
        type: 'GET',
        data: currentSearchParams,
        success: function (response) {
            console.log('AJAX 請求成功', response);

            if (response.success) {
                // 更新表格內容
                updateProductTable(response.data);

                // 更新分頁
                updatePagination(response.data);

                // 更新資料資訊
                updateDataInfo(response.data);
            } else {
                // 顯示錯誤訊息
                showMessage('錯誤', response.message || '載入商品資料失敗', 'error');

                // 清空表格
                $('#productTableBody').html('<tr><td colspan="7" class="text-center py-3">載入失敗</td></tr>');
            }
        },
        error: function (xhr, status, error) {
            console.error('AJAX 請求失敗', xhr, status, error);

            // 顯示錯誤訊息
            showMessage('錯誤', '請重新整理再嘗試', 'error');

            // 清空表格
            $('#productTableBody').html('<tr><td colspan="7" class="text-center py-3">載入失敗，請重新整理再嘗試</td></tr>');
        }
    });
}
/**
 * 更新商品表格內容
 */
function updateProductTable(data) {
    console.log('更新商品表格', data);

    const tbody = $('#productTableBody');

    // 如果沒有資料
    if (!data.items || data.items.length === 0) {
        tbody.html('<tr><td colspan="7" class="text-center py-3">沒有找到符合條件的商品</td></tr>');
        return;
    }

    // 生成表格內容
    let html = '';
    data.items.forEach(function (item) {
        html += buildProductRow(item);
    });

    tbody.html(html);
}

/**
 * 建立單一商品列的 HTML
 */
function buildProductRow(item) {
    // 建立圖片 HTML
    let imageHtml = '';
    if (item.mainImageUrl) {
        imageHtml = `<img src="${item.mainImageUrl}" class="img-fluid" style="max-height:50px; max-width:50px; object-fit: cover;" alt="商品圖片" onerror="this.style.display='none';" />`;
    }

    // 建立上架狀態切換開關
    const statusChecked = item.isActive ? 'checked' : '';
    const statusHtml = `
        <div class="form-check form-switch">
            <input class="form-check-input" type="checkbox" ${statusChecked} disabled>
        </div>
    `;

    // 建立完整的表格列
    return `
        <tr>
            <td class="text-center">${imageHtml}</td>
            <td>${escapeHtml(item.name)}</td>
            <td>${escapeHtml(item.itemNumber)}</td>
            <td>NT$${item.price.toLocaleString()}</td>
            <td>${item.quantity}</td>
            <td>${statusHtml}</td>
            <td>
                <a href="/Products/Edit/${item.id}" class="btn btn-sm btn-primary">編輯</a>
            </td>
        </tr>
    `;
}
/**
 * 更新分頁控制元件
 */
function updatePagination(data) {
    console.log('更新分頁', data);

    const pagination = $('#pagination');

    // 如果總頁數小於等於 1，隱藏分頁
    if (data.totalPages <= 1) {
        pagination.empty();
        return;
    }

    let html = '';

    // 上一頁按鈕
    if (data.hasPreviousPage) {
        html += `<li class="page-item">
                    <a class="page-link" href="#" data-page="${data.currentPage - 1}">上一頁</a>
                 </li>`;
    } else {
        html += `<li class="page-item disabled">
                    <span class="page-link">上一頁</span>
                 </li>`;
    }

    // 頁碼按鈕
    const pageNumbers = generatePageNumbers(data.currentPage, data.totalPages);
    pageNumbers.forEach(function (page) {
        if (page === '...') {
            html += `<li class="page-item disabled">
                        <span class="page-link">...</span>
                     </li>`;
        } else if (page === data.currentPage) {
            html += `<li class="page-item active">
                        <span class="page-link">${page}</span>
                     </li>`;
        } else {
            html += `<li class="page-item">
                        <a class="page-link" href="#" data-page="${page}">${page}</a>
                     </li>`;
        }
    });

    // 下一頁按鈕
    if (data.hasNextPage) {
        html += `<li class="page-item">
                    <a class="page-link" href="#" data-page="${data.currentPage + 1}">下一頁</a>
                 </li>`;
    } else {
        html += `<li class="page-item disabled">
                    <span class="page-link">下一頁</span>
                 </li>`;
    }

    pagination.html(html);
}
/**
 * 生成分頁頁碼陣列（包含省略號邏輯）
 */
function generatePageNumbers(currentPage, totalPages) {
    const pages = [];
    const showPages = 5; // 最多顯示 5 個頁碼

    if (totalPages <= showPages) {
        // 總頁數少於等於顯示頁數，全部顯示
        for (let i = 1; i <= totalPages; i++) {
            pages.push(i);
        }
    } else {
        // 總頁數較多，需要省略號邏輯
        if (currentPage <= 3) {
            // 當前頁在前面，顯示 1,2,3,4,5 ... 最後頁
            for (let i = 1; i <= 5; i++) {
                pages.push(i);
            }
            if (totalPages > 6) {
                pages.push('...');
                pages.push(totalPages);
            }
        } else if (currentPage >= totalPages - 2) {
            // 當前頁在後面，顯示 1 ... 倒數5頁
            pages.push(1);
            if (totalPages > 6) {
                pages.push('...');
            }
            for (let i = totalPages - 4; i <= totalPages; i++) {
                pages.push(i);
            }
        } else {
            // 當前頁在中間，顯示 1 ... 當前頁前後2頁 ... 最後頁
            pages.push(1);
            pages.push('...');
            for (let i = currentPage - 2; i <= currentPage + 2; i++) {
                pages.push(i);
            }
            pages.push('...');
            pages.push(totalPages);
        }
    }

    return pages;
}

/**
 * 更新資料範圍資訊顯示
 */
function updateDataInfo(data) {
    console.log('更新資料資訊', data);

    const dataInfo = $('#dataInfo');

    if (data.totalItems === 0) {
        dataInfo.text('沒有資料');
    } else {
        dataInfo.text(`顯示第 ${data.startItem} 到第 ${data.endItem} 項，共 ${data.totalItems} 項`);
    }
}

/**
 * 在頁面右上角顯示訊息提示
 */
function showMessage(title, message, type = 'info') {
    console.log('顯示訊息', title, message, type);

    // 設定不同類型的樣式
    const typeClasses = {
        'success': 'alert-success',
        'error': 'alert-danger',
        'warning': 'alert-warning',
        'info': 'alert-info'
    };

    const alertClass = typeClasses[type] || 'alert-info';
    const messageId = 'message_' + Date.now();

    // 建立訊息 HTML
    const messageHtml = `
        <div id="${messageId}" class="alert ${alertClass} alert-dismissible fade show" role="alert">
            <strong>${escapeHtml(title)}</strong> ${escapeHtml(message)}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>
    `;

    // 新增到訊息容器
    $('#messageContainer').append(messageHtml);

    // 5 秒後自動移除
    setTimeout(function () {
        $(`#${messageId}`).alert('close');
    }, 5000);
}

/**
 * HTML 字串跳脫處理（防止 XSS 攻擊）
 */
function escapeHtml(text) {
    if (!text) return '';

    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

/**
 * 重置搜尋表單
 */
function resetSearch() {
    $('#keywordInput').val('');
    $('#statusSelect').val('all');
    $('#pageSizeSelect').val('10');

    currentSearchParams = {
        keyword: '',
        status: 'all',
        page: 1,
        pageSize: 10
    };

    loadProducts();
}

/**
 * 取得當前搜尋參數（供其他模組使用）
 */
function getCurrentSearchParams() {
    return { ...currentSearchParams };
}

/**
 * 設定搜尋參數（供其他模組使用）
 */
function setSearchParams(params) {
    currentSearchParams = { ...currentSearchParams, ...params };

    // 更新表單元素
    $('#keywordInput').val(currentSearchParams.keyword);
    $('#statusSelect').val(currentSearchParams.status);
    $('#pageSizeSelect').val(currentSearchParams.pageSize);
}

// 將一些函數暴露到全域，供其他腳本使用
window.ProductSearch = {
    loadProducts: loadProducts,
    resetSearch: resetSearch,
    getCurrentSearchParams: getCurrentSearchParams,
    setSearchParams: setSearchParams,
    showMessage: showMessage
};


