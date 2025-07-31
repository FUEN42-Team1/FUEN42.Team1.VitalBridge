function ready(fileInput, showimage) {
    fileInput.change(function (event) {
        var theFile = event.target.files[0];
        const reader = new FileReader();
        reader.readAsDataURL(theFile);
        reader.addEventListener("load", event => {
            showimage.attr("src", event.target.result);
        });
    });
}
function uploadFile(fileInput,resultDiv,image,showimage) {

    if (fileInput[0].files.length === 0) {
        resultDiv[0].innerText = '請選擇檔案';
        return;
    }

    const file = fileInput[0].files[0];

    // ✅ 檢查檔案大小（10MB = 10 * 1024 * 1024 bytes）
    if (file.size > 10 * 1024 * 1024) {
        resultDiv[0].innerHTML = '<p style="color:red;">檔案大小不可超過 10MB。</p>';
        return;
    }

    // ✅ 檢查檔案類型是否為圖片
    const allowedTypes = ['image/jpeg', 'image/png', 'image/gif'];
    if (!allowedTypes.includes(file.type)) {
        resultDiv[0].innerHTML = '<p style="color:red;">只允許上傳 JPEG、PNG 或 GIF 圖片。</p>';
        return;
    }


    const formData = new FormData();
    formData.append("file", file);

    $.ajax({
        url: '/api/UploadFile/',
        type: 'POST',
        data: formData,
        processData: false, // 不處理資料
        contentType: false, // 不設定 Content-Type，讓瀏覽器自動處理
        success: function (data) {
            resultDiv[0].innerHTML = `<p>上傳成功</p>`;
            image[0].value= data.fileName;
            showimage.src = (data.filePath);
        },
        error: function (xhr, status, error) {
            const errorText = xhr.responseText || error;
            resultDiv[0].innerHTML = `<p style="color:red;">上傳失敗 ${errorText}</p>`;
        }
    });
}