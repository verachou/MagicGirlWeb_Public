// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
//點擊登出送出form
$('#anchorLogout').click(function () {
  $('#formLogout').submit();
});

//點擊登入送出form
$('#anchorLogin').click(function () {
  $('#btnLogin').click();
});
