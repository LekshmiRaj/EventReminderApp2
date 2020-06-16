
$(document).ready(function () {   
    var fbUser;                   

    $('#btnFacebookLogin').click(function () {
        fbLogin();
    });

    window.fbAsyncInit = function () {
        // FB JavaScript SDK configuration and setup
        FB.init({
            appId: '254421055834292', // FB App ID
            cookie: true,  // enable cookies to allow the server to access the session
            xfbml: true,  // parse social plugins on this page
            version: 'v3.2' // use graph api version 2.8
        });

        // Check whether the user already logged in
        FB.getLoginStatus(function (response) {
            if (response.status === 'connected') {
                //display user data
                getFbUserData();
            }
        });
    };


    // Load the JavaScript SDK asynchronously
    (function (d, s, id) {
        var js, fjs = d.getElementsByTagName(s)[0];
        if (d.getElementById(id)) return;
        js = d.createElement(s); js.id = id;
        js.src = "//connect.facebook.net/en_US/sdk.js";
        fjs.parentNode.insertBefore(js, fjs);
    }(document, 'script', 'facebook-jssdk'));


    // Facebook login with JavaScript SDK
    function fbLogin() {
        FB.login(function (response) {
            if (response.authResponse) {
                // Get and display the user profile data
                getFbUserData();
            } else {
                console.log('User cancelled login or did not fully authorize.');
            }
        }, { scope: 'email' });
    }

    function getFbUserData() {
        FB.api('/me', { locale: 'en_US', fields: 'id,first_name,last_name,email,link,gender,locale,picture' },
            function (response) {
                console.log(response); 
                fbUser = response;
                StoreAccountDetails(fbUser);
            });
    }

    function StoreAccountDetails(fbUser) {
        $.ajax({
            url: '/User/FacebookLogin',
            type: 'POST',
            data: {
                email: fbUser.email,
                name: fbUser.first_name,                      
            },
            success: function (data) {
                // window.location.href = "/User/UserHome/";
                $('#backgroundImg').css('display', 'none');
                $('#tab').css('display', 'block');
                $(btnSignInSignUp).css('display', 'none');
                $(btnSignOut).css('display', 'inline-block');
                FetchEventAndRenderCalendar();
                showList();
                $('#ModalSignInSignUp').modal('hide');
            },
        });
    }


});//document ready
