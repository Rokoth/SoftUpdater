// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.



$(document).ready(function () {    
    HideElements();
    getAuth();     
});

function getAuth() {
    $.post("/Account/CheckLogin", { check: true }).done(function (data) {             
        if (data === true) {
            ShowElements();
        }
        else {
            HideElements();
        }  
    });;
}

function ShowElements() {
    $("#logout_link").show();
    $("#home_begin_work").show();
    $("#home_db").show();
    $(".logged").show();    

    $("#login_link").hide();
    $("#home_auth").hide();
    $(".unlogged").hide();
}

function HideElements() {
    $("#logout_link").hide();
    $("#home_begin_work").hide();
    $("#home_db").hide();
    $(".logged").hide(); 

    $("#login_link").show();
    $("#home_auth").show();
    $(".unlogged").show();
}