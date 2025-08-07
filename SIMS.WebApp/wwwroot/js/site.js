// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
// site.js

$(function () { // Tương đương với $(document).ready()

    var formToSubmit; // Biến để lưu trữ form sẽ được submit

    // Bắt sự kiện click trên BẤT KỲ nút submit nào nằm trong form có class .delete-form-trigger
    // Thay vì bắt sự kiện 'submit', chúng ta bắt sự kiện 'click' trên nút bấm.
    // Điều này giúp tránh các vấn đề phức tạp liên quan đến việc ngăn chặn và kích hoạt lại sự kiện submit.
    $('.delete-form-trigger button[type="submit"]').on('click', function (e) {

        // Ngăn chặn hành vi mặc định của nút (là submit form)
        e.preventDefault();

        // Tìm đến form cha của nút bấm này và lưu lại
        formToSubmit = $(this).closest('form');

        // Lấy thông báo tùy chỉnh từ thuộc tính data-message của form
        let message = formToSubmit.data('message');

        // Nếu không có thông báo tùy chỉnh, dùng thông báo mặc định
        let defaultMessage = 'Bạn có chắc chắn muốn thực hiện hành động này không?';

        // Cập nhật nội dung của modal
        $('#deleteModalBody').text(message || defaultMessage);

        // Hiển thị modal xác nhận
        $('#confirmDeleteModal').modal('show');
    });

    // Bắt sự kiện click của nút "Xóa" bên trong modal
    $('#confirmDeleteButton').on('click', function () {
        // Nếu có một form đã được lưu, hãy submit nó
        if (formToSubmit) {
            formToSubmit.submit();
        }
    });
});