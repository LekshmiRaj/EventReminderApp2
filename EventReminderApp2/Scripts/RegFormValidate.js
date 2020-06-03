
$(document).ready(function () {
    $(".form-signup").on("submit", function (e) {
        var result = isAllInputsValid();
        if (!result) {
            e.preventDefault();
        }
    });
});


function isAllInputsValid() {
    var username = document.getElementById("user-name").value;
    var email = document.getElementById("user-email").value;
    var password = document.getElementById("user-pass").value;
    var repassword = document.getElementById("user-repeatpass").value;

    var isValid = true;

    var mailformat = /^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*(\.\w{2,3})+$/;
    if (!email.match(mailformat)) {
        alert("You have entered an invalid email address!");
        document.getElementById("user-email").focus();
        isValid = false;
    }

    if (!password.match(repassword)) {
        alert("Password does not match");
        document.getElementById("user-repeatpass").focus();
        isValid = false;
    }

    return isValid;
}


