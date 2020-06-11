$(document).ready(function () {

    // for google authentication
    var OAUTHURL = 'https://accounts.google.com/o/oauth2/auth?';
    var VALIDURL = 'https://www.googleapis.com/oauth2/v1/tokeninfo?access_token=';
    var SCOPE = 'https://www.googleapis.com/auth/userinfo.profile https://www.googleapis.com/auth/userinfo.email';
    var CLIENTID = '718508958029-49a98067qtqp248e6h0fv752tg4u076d.apps.googleusercontent.com';
    var REDIRECT = 'http://localhost:60256/User/UserHome';
    var LOGOUT = 'http://localhost:60256/User/UserHome';
    var TYPE = 'token';
    var _url = OAUTHURL + 'scope=' + SCOPE + '&client_id=' + CLIENTID + '&redirect_uri=' + REDIRECT + '&response_type=' + TYPE;
    var acToken;
    var tokenType;
    var expiresIn;
    var user;
    var loggedIn = false;
    ////////

    
    $('#backgroundImg').css('display', 'block');
    $('#tab').css('display', 'none');

    ////login////
    $('#btnSignInSignUp').click(function () {
        //Open modal dialog for signin/signup
        $('#ModalSignInSignUp').modal();
    })

    $('#btnLogin').click(function () {
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
                    //Refresh the calender
                    FetchEventAndRenderCalendar();
                    showList();
                    $('#ModalSignInSignUp').modal('hide');

                }
            },
            error: function () {
                alert('Failed');
            }
        })
    }


    ///register///
    $('#btn-SignUp').click(function () {
        var RegData = {
            UserName: $('#user-name').val(),
            Email: $('#user-email').val(),
            Password: $('#user-pass').val()
        }
        Register(RegData);
    })

    function Register(data) {
        $.ajax({
            type: "POST",
            url: '/User/SignUp',
            data: data,
            success: function (data) {
                if (data.status) {
                    //Refresh the calender
                    // FetchEventAndRenderCalendar();
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
    //FetchEventAndRenderCalendar();
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
            contentHeight: 400,
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
                $description.append($('<p/>').html('<b>Start:</b>' + calEvent.start.format("DD-MMM-YYYY HH:mm a")));
                if (calEvent.end != null) {
                    $description.append($('<p/>').html('<b>End:</b>' + calEvent.end.format("DD-MMM-YYYY HH:mm a")));
                }
                $description.append($('<p/>').html('<b>Description:</b>' + calEvent.description));
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
                    StartDate: event.start.format('YYYY/MM/DD HH:mm A'),
                    EndDate: event.end != null ? event.end.format('YYYY/MM/DD HH:mm A') : null,
                    Description: event.description
                };
                SaveEvent(data);
            }

        })
    }


    ////create event////       
    $('#btnSubmitCreate').click(function () {
        //Validation/
        if ($('#EventName').val().trim() == "") {
            alert('EventName is required');
            return;
        }
        if ($('#StartDate').val().trim() == "") {
            alert('Start date required');
            return;
        }

        else {
            var startDate = moment($('#StartDate').val(), "DD/MM/YYYY HH:mm A").toDate();
            var endDate = moment($('#EndDate').val(), "DD/MM/YYYY HH:mm A").toDate();
            if (startDate > endDate) {
                alert('Invalid end date');
                return;
            }
        }
        var data = {
            EventId: $('#EventID').val(),
            EventName: $('#EventName').val().trim(),
            Description: $('#Description').val(),
            StartDate: $('#StartDate').val().trim(),
            EndDate: $('#EndDate').val().trim()
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
                if (data.status) {                           
                    //Refresh the calender
                    FetchEventAndRenderCalendar();
                }
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
                if (data.status) {
                    //Refresh the calender
                    FetchEventAndRenderCalendar();
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
                        //Refresh the calender
                        FetchEventAndRenderCalendar();
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
    })

    function openAddEditForm() {
        if (selectedEvent != null) {
            $('#hdEventID').val(selectedEvent.eventID);
            $('#txtSubject').val(selectedEvent.title);
            $('#txtStart').val(selectedEvent.start.format('DD/MM/YYYY HH:mm A'));
            $('#txtEnd').val(selectedEvent.end != null ? selectedEvent.end.format('DD/MM/YYYY HH:mm A') : '');
            $('#txtDescription').val(selectedEvent.description);
        }
        $('#myModal').modal('hide');
        $('#myModalSave').modal();
    }



    $('#btnSave').click(function () {
        //Validation/
        if ($('#txtSubject').val().trim() == "") {
            alert('Subject required');
            return;
        }
        if ($('#txtStart').val().trim() == "") {
            alert('Start date required');
            return;
        }

        else {
            var startDate = moment($('#txtStart').val(), "DD/MM/YYYY HH:mm A").toDate();
            var endDate = moment($('#txtEnd').val(), "DD/MM/YYYY HH:mm A").toDate();
            if (startDate > endDate) {
                alert('Invalid end date');
                return;
            }
        }
        var data = {
            EventId: $('#hdEventID').val(),
            EventName: $('#txtSubject').val().trim(),
            Description: $('#txtDescription').val(),
            StartDate: $('#txtStart').val().trim(),
            EndDate: $('#txtEnd').val().trim()
        }
        SaveEvent(data);
    })


    function showList() {
        $.ajax({
            type: "GET",
            url: "/User/GetEvents",
            success: function (data) {
                $.each(data, function (i, item) {
                    var rows = "<tr>"
                        + "<td>" + i + "</td>"
                        + "<td>" + item.EventId + "</td>"
                        + "<td>" + item.EventName + "</td>"
                        + "<td>" + item.Description + "</td>"
                        + "<td>" + item.StartDateStr + "</td>"
                        + "<td>" + item.EndDateStr + "</td>"
                        + "</tr>";
                    $('#tblEventList tbody').append(rows);
                })

                GenerateCalender(events);
            },
            error: function (error) {
                alert('failed');
            }
        });
    }


    //signOut
    $('#btnSignOut').click(function () {       
        ClearSession();
        $('#backgroundImg').css('display', 'block');
        $('#tab').css('display', 'none');
        $(btnSignOut).css('display', 'none');
        $(btnSignInSignUp).css('display', 'inline-block');
        $('#inputEmail').val("");  
        $('#inputPassword').val(""); 
    });

    function ClearSession() {
        $.ajax({
            type: "GET",
            url: "/User/ClearSessions",
            success: function (data) {                                
            },
            error: function (error) {
                alert('failed');
            }
        });
    }



    //google authentication
    $('#btnGoogleLogin').click(function () {
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


})//document.ready       


