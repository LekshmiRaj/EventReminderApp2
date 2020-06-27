
$(document).ready(function () {
    $(".PasswordResetForm").on("submit", function (e) {
        var result = isInputsValid();
        if (!result) {
            e.preventDefault();
        }
    });
});


function isInputsValid() {    
    var password = document.getElementById("NewPassword").value;
    var repassword = document.getElementById("ConfirmPassword").value;

    var isValid = true;
    
    if (!password.match(repassword)) {
        alert("Password does not match");
        document.getElementById("ConfirmPassword").focus();
        isValid = false;
    }

    return isValid;
}


