
$(document).ready(function () {
    $('#btnFacebookLogin').click(function () {
        
        FB.login(function (response) {
            console.log(response);
            if (response.status === 'connected') {   // Logged into your webpage and Facebook.
                testAPI();
            } 
        });

        function testAPI() {                      // Testing Graph API after login.  See statusChangeCallback() for when this call is made.
            console.log('Welcome!  Fetching your information.... ');
            FB.api('/me', function (response) {
                console.log('Successful login for: ' + response.name);   
                console.log(response); 
            });

        }
    });
});
