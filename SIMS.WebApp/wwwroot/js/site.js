// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
// site.js
$(function () {
    var formToSubmit;

    // Đổi tên class trigger để dùng chung cho nhiều hành động
    $('.delete-form-trigger button[type="submit"]').on('click', function (e) {
        e.preventDefault();
        formToSubmit = $(this).closest('form');

        // Lấy thông báo và hành động từ thuộc tính data-* của button
        let message = formToSubmit.data('message') || 'Are you sure you want to perform this action?';
        let action = $(this).text().trim(); // Lấy text của nút bấm (Block, Unblock)
        let confirmButton = $('#confirmActionButton');

        // Cập nhật nội dung modal
        $('#deleteModalBody').text(message);

        // Cập nhật nút xác nhận
        confirmButton.text(action);

        // Cập nhật màu sắc nút cho phù hợp
        confirmButton.removeClass('btn-danger btn-success').addClass(action === 'Block' ? 'btn-danger' : 'btn-success');

        $('#confirmDeleteModal').modal('show');
    });

    $('#confirmActionButton').on('click', function () {
        if (formToSubmit) {
            formToSubmit.submit();
        }
    });
});

