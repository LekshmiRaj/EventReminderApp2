$(document).ready(function () {

    // for google authentication
    var OAUTHURL = 'https://accounts.google.com/o/oauth2/auth?';
    var VALIDURL = 'https://www.googleapis.com/oauth2/v1/tokeninfo?access_token=';
    var SCOPE = 'https://www.googleapis.com/auth/userinfo.profile https://www.googleapis.com/auth/userinfo.email';
    var CLIENTID = '718508958029-49a98067qtqp248e6h0fv752tg4u076d.apps.googleusercontent.com';
    var REDIRECT = 'http://localhost:60256/User/UserHome';   
    var TYPE = 'token';
    var _url = OAUTHURL + 'scope=' + SCOPE + '&client_id=' + CLIENTID + '&redirect_uri=' + REDIRECT + '&response_type=' + TYPE;
    var acToken;    
    var user;    
    ////////

    var save;
    var fbUser;
    var userType;   
    
    $('#backgroundImg').css('display', 'block');
    $('#tab').css('display', 'none');

    if ($('#SessionUserId').val() != null && $('#SessionEmail').val() != null) {

        $('#backgroundImg').css('display', 'none');
        $('#tab').css('display', 'block');
        $(btnSignInSignUp).css('display', 'none');
        $(btnSignOut).css('display', 'inline-block');
        $('#welcome').css('display', 'inline-block');
        FetchEventAndRenderCalendar();
        showList();
        $('#currentUser').text($('#SessionUserName').val());
        $('#ModalSignInSignUp').modal('hide');               
    }

    ////login////
    $('#btnSignInSignUp').click(function () {
        //Open modal dialog for signin/signup
        $('#ModalSignInSignUp').modal();
    })

    $('#btnLogin').click(function () {
        userType = 'normal';

        if ($('#inputEmail').val() == "") {
            alert("Email Id is required");
            return;
        }

        if ($('#inputPassword').val() == "") {
            alert("Password is required");
            return;
        }

        currentUserEmail = $('#inputEmail').val();
        var data = {
            Email: $('#inputEmail').val(),
            Password: $('#inputPassword').val()
        }
        Login(data);
    });

    function Login(loginData) {
        $.ajax({
            type: "POST",
            url: '/User/SignIn',
            data: loginData,
            success: function (data) {
                if (data.status) {
                    $('#backgroundImg').css('display', 'none');
                    $('#tab').css('display', 'block');
                    $(btnSignInSignUp).css('display', 'none');
                    $(btnSignOut).css('display', 'inline-block');
                    $('#welcome').css('display', 'inline-block');
                    //Refresh the calender
                    FetchEventAndRenderCalendar();
                    showList();
                    $('#currentUser').text(data.username);
                    $('#ModalSignInSignUp').modal('hide');
                } else {
                    toastr.warning("Invalid email or password", "Failed");
                }
            },
            error: function () {
                alert('Failed');
            }
        })
    }


    function registerValidation() {

        var isValid = true;

        if ($('#user-name').val() == "") {
            alert("User name is required");
            isValid = false;
        }

        var mailformat = /^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*(\.\w{2,3})+$/;
        if (!$('#user-email').val().match(mailformat)) {
            alert("You have entered an invalid email address!");
            document.getElementById("user-email").focus();
            isValid = false;
        }

        var no = $('#user-phone').val();

        if (!no == "") {
            var numbers = /^[0-9]+$/;
            if (no.match(numbers)) {
                if (no.length > 10 || no.length < 10) {
                    alert('Invalid phone number');
                    document.getElementById("user-phone").focus();
                    isValid = false;
                }
            }
            else {
                alert('Invalid phone number');
                document.getElementById("user-phone").focus();
                isValid = false;
            }
        }

        if ($('#user-pass').val() == "") {
            alert("Password is required");
            isValid = false;
        }
        if ($('#user-repeatpass').val() == "") {
            alert("Confirm password is required");
            isValid = false;
        }
        //if (!$('#user-pass').val().match($('#user-repeatpass').val()) && !$('#user-pass').val().match($('#user-repeatpass').val())) {
        //    alert("Password does not match");
        //    document.getElementById("user-repeatpass").focus();
        //    isValid = false;
        //}

        if ($('#user-pass').val()!=$('#user-repeatpass').val()) {
            alert("Password does not match");
            document.getElementById("user-repeatpass").focus();
            isValid = false;
        }
       
        return isValid;
    }
    ///register///
    $('#btn-SignUp').click(function () {       

        if (registerValidation()) { 
            var RegData = {
                UserName: $('#user-name').val(),
                Email: $('#user-email').val(),
                Password: $('#user-pass').val(),
                DOB: $('#user-dob').val(),
                Phone: $('#user-phone').val()
            }
        //clearing form values
        $('#user-name').val("");
        $('#user-email').val("");
        $('#user-pass').val("");
        $('#user-repeatpass').val("");
        $('#user-dob').val("");
        $('#user-phone').val("");
            Register(RegData);
        }         
    });

    function Register(data) {
        $.ajax({
            type: "POST",
            url: '/User/SignUp',
            data: data,
            success: function (data) {
                if (data.status) {
                    toastr.success("You are registered sucessfully...", "Sucess");
                    $('#ModalSignInSignUp').modal();
                }
            },
            error: function () {
                alert('Failed');
            }
        })
    }


    ///calnder///
    var events = [];
    var selectedEvent = null;   
    function FetchEventAndRenderCalendar() {
        events = [];
        $.ajax({
            type: "GET",
            url: "/User/GetEvents",
            success: function (data) {
                $.each(data, function (i, v) {
                    events.push({
                        eventID: v.EventId,
                        title: v.EventName,
                        description: v.Description,
                        start: moment(v.StartDate),
                        end: v.EndDate != null ? moment(v.EndDate) : null
                    });
                })

                GenerateCalender(events);
            },
            error: function (error) {
                alert('failed');
            }
        })
    }


    function GenerateCalender(events) {
        $('#calender').fullCalendar('destroy');
        $('#calender').fullCalendar({
            contentHeight: 470,
            defaultDate: new Date(),
            timeFormat: 'h(:mm)a',
            header: {
                left: 'prev,next today',
                center: 'title',
                right: 'month,basicWeek,basicDay,agenda'
            },
            eventLimit: true,
            eventColor: 'orange',
            events: events,
            eventClick: function (calEvent, jsEvent, view) {
                selectedEvent = calEvent;
                $('#myModal #eventTitle').text(calEvent.title);
                var $description = $('<div/>');
                $description.append($('<p/>').html('<b>Start: </b>' + calEvent.start.format("DD-MM-YYYY HH:mm a")));
                if (calEvent.end != null) {
                    $description.append($('<p/>').html('<b>End: </b>' + calEvent.end.format("DD-MM-YYYY HH:mm a")));
                }
                $description.append($('<p/>').html('<b>Description: </b>' + calEvent.description));
                $('#myModal #pDetails').empty().html($description);

                $('#myModal').modal();
            },
            selectable: true,
            select: function (start, end) {
                selectedEvent = {
                    eventID: 0,
                    title: '',
                    description: '',
                    start: start,
                    end: end
                };
                openAddEditForm();
                $('#calendar').fullCalendar('unselect');
            },
            editable: true,
            eventDrop: function (event) {
                var data = {
                    EventId: event.eventID,
                    EventName: event.title,
                    StartDate: event.start.format('DD-MM-YYYY HH:mm a'),
                    EndDate: event.end != null ? event.end.format('DD-MM-YYYY HH:mm a') : null,
                    Description: event.description
                };
                SaveEvent(data);
            }

        });
    }


    ////create event////       
    $('#btnSubmitCreate').click(function () {
        //Validation/        
        var date1 = $('#StartDate').val();
        var date2 = $('#EndDate').val();        

        if ($('#EventName').val().trim() == "") {
            alert('Subject is required');
            return;
        }
        if ($('#StartDate').val().trim() == "") {
            alert('Start date is required');
            return;
        }

        if ($('#EndDate').val().trim() == "") {
            alert('End date is required');
            return;
        }

        if (new Date(date1).getDate() == new Date(date2).getDate()) {
          if (new Date(date1).getTime() == new Date(date2).getTime()) {
              alert('Start date should not be greater than or equal to end date');
              return;
          }
        }
                    
        if (new Date(date1) > new Date(date2)){
            alert('Start date should not be greater than or equal to end date');
                return;
        }
                   
            var selectedDate = new Date(date1);
            var now = new Date();
            if (selectedDate < now) {
                if (!confirm("Selected date is in the past. Do you still want to create this event?")) {
                    return;
                }
            }
                     
        var data = {
            EventId: $('#EventID').val(),
            EventName: $('#EventName').val().trim(),
            Description: $('#Description').val(),
            StartDate: $('#StartDate').val(),
            EndDate: $('#EndDate').val()                       
        }       
        //clearing form values
        $('#EventID').val("");
        $('#EventName').val("");
        $('#Description').val("");
        $('#StartDate').val("");
        $('#EndDate').val("");
        
        CreateEvent(data);        
    })
    

    function CreateEvent(data) {
        $.ajax({
            type: "POST",
            url: '/User/SaveEvent',
            data: data,
            success: function (data) {                                                        
                toastr.success("Event created sucessfully...", "Sucess");
                FetchEventAndRenderCalendar();
                showList();   
                $('#createEventModal').modal('hide'); 
            },
            error: function () {
                alert('Failed');
            }
        })
    }


    function SaveEvent(data) {
        $.ajax({
            type: "POST",
            url: '/User/SaveEvent',
            data: data,
            success: function (data) {               
                    FetchEventAndRenderCalendar();
                    showList();
                    if (save == "UpdateEve") {
                        toastr.success("Event Updated sucessfully...", "Update");
                        $('#ModalListUpdate').modal('hide');
                    } else {
                        $('#myModalSave').modal('hide');
                    }                                  
            },
            error: function () {
                alert('Failed');
            }
        })
    }

    //delete in popup
    $('#btnDelete').click(function () {
        if (selectedEvent != null && confirm('Are you sure?')) {
            $.ajax({
                type: "POST",
                url: "/User/DeleteEvent",
                data: { 'eventID': selectedEvent.eventID },
                success: function (data) {
                    if (data.status) {
                        toastr.warning("Event deleted sucessfully...", "Delete");
                        //Refresh the calender
                        FetchEventAndRenderCalendar();
                        showList();
                        $('#myModal').modal('hide');
                    }
                },
                error: function (a, b, c) {
                    alert('Failed, ' + c);
                }
            })
        }
    })


    $('#btnEdit').click(function () {
        //Open modal dialog for edit event
        openAddEditForm();
    });

    function openAddEditForm() {
        if (selectedEvent != null) {
            $('#hdEventID').val(selectedEvent.eventID);
            $('#txtSubject').val(selectedEvent.title);
            $('#txtStart').val(selectedEvent.start.format('DD-MM-YYYY HH:mm'));
            $('#txtEnd').val(selectedEvent.end != null ? selectedEvent.end.format('DD-MM-YYYY HH:mm') : '');
            $('#txtDescription').val(selectedEvent.description);
        }
        $('#myModal').modal('hide');
        $('#myModalSave').modal();
    }



    $('#btnSave').click(function () {
        //Validation/
        var date1 = $('#txtStart').val();
        var date2 = $('#txtEnd').val();

        //Validation/
        if ($('#txtSubject').val().trim() == "") {
            alert('Subject required');
            return;
        }
        if ($('#txtStart').val().trim() == "") {
            alert('Start date is required');
            return;
        }

        if ($('#txtEnd').val().trim() == "") {
            alert('End date is required');
            return;
        }

        var d1 = moment($('#txtStart').val(), "DD-MM-YYYY HH:mm",true);
        var d2 = moment($('#txtEnd').val(), "DD-MM-YYYY HH:mm", true);        
        if (d1.isValid() == false || d2.isValid() == false) {
            alert("Invalid start or end date");
            return;
        }
                        
        var startDate = moment($('#txtStart').val(), "DD-MM-YYYY HH:mm a").toDate();
        var endDate = moment($('#txtEnd').val(), "DD-MM-YYYY HH:mm a").toDate();
        if (startDate >= endDate) {
            alert('Start date should not be greater than or equal to end date');
            return;
        }
              
        var selectedDate = new Date(date1);
        var now = new Date();
        if (selectedDate < now) {
            if (!confirm("Selected date is in the past. Do you still want to create this event?")) {
                return;
            }
        }
        
        var data = {
            EventId: $('#hdEventID').val(),
            EventName: $('#txtSubject').val().trim(),
            Description: $('#txtDescription').val(),
            StartDate: $('#txtStart').val(),
            EndDate: $('#txtEnd').val()
        }
        SaveEvent(data);
    });
  

    function showList() {
        $('#tblEventList tbody').empty();
        $.ajax({
            type: "GET",
            url: "/User/GetEvents",
            success: function (data) {
                $.each(data, function (i, item) {
                       var rows = "<tr id=" + item.EventId +">"
                           + "<td>" + ++i + "</td>"
                        + "<td>" + item.EventId + "</td>"
                        + "<td>" + item.EventName + "</td>"
                        + "<td>" + item.Description + "</td>"
                        + "<td>" + item.StartDateStr + "</td>"
                        + "<td>" + item.EndDateStr + "</td>"
                        + "<td>" + "<button class='EditRow'" + ">Edit</button>" + "</td>"
                        + "<td>" + "<button class='deleteRow'" + ">Delete</button>" + "</td>"
                        + "</tr>";
                    $('#tblEventList tbody').append(rows);
                }); 

                $('#tblEventList tbody .EditRow').click(function () {
                    var eveId = $(this).closest('tr').attr("id");
                    openEditPopUp(eveId);                                       
                });

                $("#tblEventList tbody .deleteRow").click(function () {                    
                    var eventId = $(this).closest('tr').attr("id");
                    var confirmDelete = confirm('Are you sure you want to delete this?');
                    if (confirmDelete == true) {
                        $.ajax(
                            {
                                type: "POST",
                                url: "User/DeleteEvent",
                                data: { 'eventID': eventId },
                                success: function (result) {
                                    toastr.warning("Event deleted sucessfully...", "Delete");
                                    $("#" + eventId).remove();
                                    FetchEventAndRenderCalendar();
                                },
                                error: function (a, b, c) {
                                    alert(c);
                                }
                            });

                    } else {
                        return false;
                    }
                });
            },
            error: function (error) {
                alert('Failed');
            }
        });
    }

          
    //signOut
    $('#btnSignOut').click(function () {       

        if (confirm('Are you sure you want to log out?')) {
            ClearSession();
            $('#backgroundImg').css('display', 'block');
            $('#tab').css('display', 'none');
            $(btnSignOut).css('display', 'none');
            $(btnSignInSignUp).css('display', 'inline-block');
            $('#welcome').css('display', 'none');
            $('#inputEmail').val("");
            $('#inputPassword').val("");
            $("#currentUser").empty();
            if (userType == 'facebook') {
                FB.logout(function (response) {
                    console.log('facebook logout');
                });
            }
        }
    });

    function ClearSession() {
        $.ajax({
            type: "GET",
            url: "/User/ClearSessions",
            success: function (data) {                                
            },
            error: function (error) {
                alert('Failed');
            }
        });
    }
   

    //Update event list
    
    $('#btnUpdateList').click(function () {

        save = "UpdateEve";

        var date1 = $('#StartDateList').val();
        var date2 = $('#EndDateList').val();  
                                
        //Validation/
        if ($('#EventNameList').val().trim() == "") {
            alert('Subject required');
            return;
        }
        if ($('#StartDateList').val().trim() == "") {
            alert('Start date is required');
            return;
        }
        if ($('#EndDateList').val().trim() == "") {
            alert('End date is required');
            return;
        }

        var d1 = moment($('#StartDateList').val(), "DD-MM-YYYY HH:mm", true);
        var d2 = moment($('#EndDateList').val(), "DD-MM-YYYY HH:mm", true);
        if (d1.isValid() == false || d2.isValid() == false) {
            alert("Invalid start or end date");
            return;
        }
             
         var startDate = moment($('#StartDateList').val(), "DD-MM-YYYY HH:mm a").toDate();
         var endDate = moment($('#EndDateList').val(), "DD-MM-YYYY HH:mm a").toDate();
         if (startDate >= endDate) {
             alert('Start date should not be greater than or equal to end date');
             return;
        }
                        
        var selectedDate = new Date(date1);
        var now = new Date();
        if (selectedDate < now) {
            if (!confirm("Selected date is in the past. Do you still want to create this event?")) {
                return;
            }
        }
        
        var data = {
            EventId: $('#EventIdList').val(),
            EventName: $('#EventNameList').val().trim(),
            Description: $('#DescriptionList').val(),
            StartDate: $('#StartDateList').val(),
            EndDate: $('#EndDateList').val()
        }
        SaveEvent(data);   
        
    });

    function openEditPopUp(eveId) {
        $.ajax({
            type: "POST",
            url: "/User/Edit",
            data: { 'id': eveId },
            success: function (eventModel) {
                    var startDate = moment(eventModel.StartDate).format('DD-MM-YYYY HH:mm');
                    var endDate = moment(eventModel.EndDate).format('DD-MM-YYYY HH:mm');
                    $('#EventIdList').val(eventModel.EventId);
                    $('#EventNameList').val(eventModel.EventName);
                    $('#StartDateList').val(startDate);
                    $('#EndDateList').val(eventModel.EndDate != null ? endDate : '');
                    $('#DescriptionList').val(eventModel.Description);                

                    $('#ModalListUpdate').modal();
            },
            error: function (a, b, c) {
                alert('Failed, ' + c);
            }
        });                                          
    }

    //forgot password
    $("#btnResetPassword").click(function () {
        var data = {
            Email: $('#resetEmail').val()           
        }
        forgotPassword(data);
    });

    function forgotPassword(data) {
        $.ajax({
            type: "POST",
            url: '/User/ForgotPassword',
            data: data,
            success: function (data) {
                if (data.status) {
                    toastr.success("A link is send to your registered email", "Check mail");
                }
                else {
                    toastr.warning("Incorrect mail id", "Not found");
                }
            },
            error: function () {
                alert('Failed');
            }
        })
    }


    //google authentication
    $('#btnGoogleLogin').click(function () {
        userType = 'google';
        login();
    });

    function login() {
        var win = window.open(_url, "windowname1", 'width=800, height=600');
        var pollTimer = window.setInterval(function () {
            try {
                console.log(win.document.URL);
                if (win.document.URL.indexOf(REDIRECT) != -1) {
                    window.clearInterval(pollTimer);
                    var url = win.document.URL;
                    acToken = gup(url, 'access_token');
                    tokenType = gup(url, 'token_type');
                    expiresIn = gup(url, 'expires_in');

                    win.close();
                    debugger;
                    validateToken(acToken);
                }
            }
            catch (e) {

            }
        }, 500);
    }


    function gup(url, name) {
        namename = name.replace(/[\[]/, "\\\[").replace(/[\]]/, "\\\]");
        var regexS = "[\\#&]" + name + "=([^&#]*)";
        var regex = new RegExp(regexS);
        var results = regex.exec(url);
        if (results == null)
            return "";
        else
            return results[1];
    }

    function validateToken(token) {
        getUserInfo();
        $.ajax(
            {
                url: VALIDURL + token,
                data: null,
                success: function (responseText) {
                },
            });
    }


    function getUserInfo() {
        $.ajax({
            url: 'https://www.googleapis.com/oauth2/v1/userinfo?access_token=' + acToken,
            data: null,
            success: function (resp) {
                user = resp;
                $('#currentUser').text(user.name);
                $('#welcome').css('display', 'inline-block');
                getAccountDetails(user);
                console.log(user);
            }
        });            
    }


    function getAccountDetails(user) {
        $.ajax({
            url: '/User/GoogleLogin',
            type: 'POST',
            data: {
                email: user.email,
                name: user.name,
                gender: user.gender,
                lastname: user.lastname,
                location: user.location
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
    

    //////facebook authentication


    // Load the JavaScript SDK asynchronously
    (function (d, s, id) {
        var js, fjs = d.getElementsByTagName(s)[0];
        if (d.getElementById(id)) return;
        js = d.createElement(s); js.id = id;
        js.src = "//connect.facebook.net/en_US/sdk.js";
        fjs.parentNode.insertBefore(js, fjs);
    }(document, 'script', 'facebook-jssdk'));

    $('#btnFacebookLogin').click(function () {
        userType = 'facebook';
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
                $('#currentUser').text(fbUser.first_name);
                $('#welcome').css('display', 'inline-block');
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
                FetchEventAndRenderCalendar();
                showList();
                $('#backgroundImg').css('display', 'none');
                $('#tab').css('display', 'block');
                $(btnSignInSignUp).css('display', 'none');
                $(btnSignOut).css('display', 'inline-block');                
                $('#ModalSignInSignUp').modal('hide');
            },
        });
    }

    $('#currentUser').click(function () {
        openUserDetailsEditPopUp();
    });

    function openUserDetailsEditPopUp() {
        $.ajax({
            type: "POST",
            url: "/User/GetUserDetails",            
            success: function (regUser) {
                $('#uId').val(regUser.UserId);
                $('#uName').val(regUser.UserName);
                $('#uEmail').val(regUser.Email);
                $('#uDOB').val(regUser.DobString);
                $('#uPhone').val(regUser.Phone);
                $('#ModalUserDetails').modal(); 
            },
            error: function (a, b, c) {
                alert('Failed, ' + c);
            }
        });
    }
   
    $('#btnEditUserDetails').click(function () {
       
        //Validation/
        if ($('#uName').val().trim() == "") {
            alert('User name required');
            return;
        }
        if ($('#uEmail').val().trim() == "") {
            alert('Email required');
            return;
        }

        else
        {
        var mailformat = /^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*(\.\w{2,3})+$/;
            if (!$('#uEmail').val().match(mailformat))
            {b
            alert("You have entered an invalid email address!");
            return;            
            }
        }

        var dob = moment($('#uDOB').val(), "DD-MM-YYYY", true);
        if (dob.isValid() == false) {
            alert("Invalid date of birth");
            return;
        }

        var no = $('#uPhone').val();
        if (no.length != 0) {
            var numbers = /^[0-9]+$/;
            if (no.match(numbers)) {
                if (no.length > 10 || no.length < 10) {
                    alert('Invalid phone number');
                    document.getElementById("user-phone").focus();
                    return;
                }
            }
            else {
                alert('Invalid phone number');
                document.getElementById("user-phone").focus();
                return;
            }
        }
        
        var userData = {
            UserId: $('#uId').val(),
            UserName: $('#uName').val().trim(),
            Email: $('#uEmail').val(),
            DOB: $('#uDOB').val(),
            Phone: $('#uPhone').val()
        }
        UpdateUser(userData);

    });

    function UpdateUser(userData) {
        $.ajax({
            type: "POST",
            url: '/User/UpdateUserDetails',
            data: userData,
            success: function (data) {
                if (data.status) {
                    toastr.success("Updated sucessfully...", "Sucess");
                    $('#currentUser').text(data.username);
                    $('#ModalUserDetails').modal('hide'); 
                }
            },
            error: function () {
                alert('Failed');
            }
        })
    }

    $('#btnCreateEvent').click(function () {
        $('#createEventModal').modal();       
    });
       
})//document.ready       


