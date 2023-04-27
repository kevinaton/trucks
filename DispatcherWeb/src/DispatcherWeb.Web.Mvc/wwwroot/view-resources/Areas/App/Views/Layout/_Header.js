(function ($) {
    $(function () {

        //Back to my account

        $('#UserProfileBackToMyAccountButton').click(function (e) {
            e.preventDefault();
            abp.ajax({
                url: abp.appPath + 'Account/BackToImpersonator',
                success: function () {
                    app.localStorage.clear();
                    if (!app.supportsTenancyNameInUrl) {
                        abp.multiTenancy.setTenantIdCookie(abp.session.impersonatorTenantId);
                    }
                }
            });
        });

        $('a[href="/Account/Logout"]').click(function () {
            app.localStorage.clear();
        });

        //My settings

        var mySettingsModal = new app.ModalManager({
            viewUrl: abp.appPath + 'App/Profile/MySettingsModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/App/Views/Profile/_MySettingsModal.js',
            modalClass: 'MySettingsModal'
        });

        $('#UserProfileMySettingsLink').click(function (e) {
            e.preventDefault();
            mySettingsModal.open();
        });

        $('#UserDownloadCollectedDataLink').click(function (e) {
            e.preventDefault();
            abp.services.app.profile.prepareCollectedData().done(function () {
                abp.message.success(app.localize("GdprDataPrepareStartedNotification"));
            });
        });

        //Change password

        var changePasswordModal = new app.ModalManager({
            viewUrl: abp.appPath + 'App/Profile/ChangePasswordModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/App/Views/Profile/_ChangePasswordModal.js',
            modalClass: 'ChangePasswordModal'
        });

        $('#UserProfileChangePasswordLink').click(function (e) {
            e.preventDefault();
            changePasswordModal.open();
        });

        //Change profile picture

        var changeProfilePictureModal = new app.ModalManager({
            viewUrl: abp.appPath + 'App/Profile/ChangePictureModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/App/Views/Profile/_ChangePictureModal.js',
            modalClass: 'ChangeProfilePictureModal'
        });

        $('#UserProfileChangePictureLink').click(function (e) {
            e.preventDefault();
            changeProfilePictureModal.open();
        });

        //Upload signature picture

        var uploadSignaturePictureModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Profile/UploadSignaturePictureModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Profile/_UploadSignaturePictureModal.js',
            modalClass: 'UploadSignaturePictureModal'
        });

        $('#UserUploadSignaturePictureLink').click(function (e) {
            e.preventDefault();
            uploadSignaturePictureModal.open();
        });

        //Manage linked accounts
        var _userLinkService = abp.services.app.userLink;

        var manageLinkedAccountsModal = new app.ModalManager({
            viewUrl: abp.appPath + 'App/Profile/LinkedAccountsModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/App/Views/Profile/_LinkedAccountsModal.js',
            modalClass: 'LinkedAccountsModal'
        });

        var getRecentlyLinkedUsers = function () {
            _userLinkService.getRecentlyUsedLinkedUsers()
                .done(function (result) {

                    loadRecentlyUsedLinkedUsers(result);

                    $("#ManageLinkedAccountsLink").click(function (e) {
                        e.preventDefault();
                        manageLinkedAccountsModal.open();
                    });

                    $(".recently-linked-user").click(function (e) {
                        e.preventDefault();
                        var userId = $(this).attr("data-user-id");
                        var tenantId = $(this).attr("data-tenant-id");
                        if (userId) {
                            switchToUser(userId, tenantId);
                        }
                    });
                });
        };

        function loadRecentlyUsedLinkedUsers(result) {
            var $ul = $("ul#RecentlyUsedLinkedUsers");

            $.each(result.items, function (index, linkedUser) {
                linkedUser.shownUserName = app.getShownLinkedUserName(linkedUser);
            });

            result.hasLinkedUsers = function () {
                return this.items.length > 0;
            };

            var template = $('#linkedAccountsSubMenuTemplate').html();
            Mustache.parse(template);
            var rendered = Mustache.render(template, result);
            $ul.html(rendered);
        }

        function switchToUser(linkedUserId, linkedTenantId) {
            abp.ajax({
                url: abp.appPath + 'Account/SwitchToLinkedAccount',
                data: JSON.stringify({
                    targetUserId: linkedUserId,
                    targetTenantId: linkedTenantId
                }),
                success: function () {
                    if (!app.supportsTenancyNameInUrl) {
                        abp.multiTenancy.setTenantIdCookie(linkedTenantId);
                    }
                }
            });
        }

        manageLinkedAccountsModal.onClose(function () {
            getRecentlyLinkedUsers();
        });

        //Notifications
        var _appUserNotificationHelper = new app.UserNotificationHelper();
        var _notificationService = abp.services.app.notification;

        function bindNotificationEvents() {
            $('#setAllNotificationsAsReadLink').click(function (e) {
                e.preventDefault();
                e.stopPropagation();

                _appUserNotificationHelper.setAllAsRead(function () {
                    loadNotifications();
                });
            });

            $('.set-notification-as-read').click(function (e) {
                e.preventDefault();
                e.stopPropagation();

                var notificationId = $(this).attr("data-notification-id");
                if (notificationId) {
                    _appUserNotificationHelper.setAsRead(notificationId, function () {
                        loadNotifications();
                    });
                }
            });

            $('#openNotificationSettingsModalLink').click(function (e) {
                e.preventDefault();
                e.stopPropagation();

                _appUserNotificationHelper.openSettingsModal();
            });

            $('div.user-notification-item-clickable').click(function () {
                var url = $(this).attr('data-url');
                document.location.href = url;
            });
        }

        function loadNotifications() {
            _notificationService.getUserNotifications({
                maxResultCount: 3
            }).done(function (result) {
                result.notifications = [];
                result.unreadMessageExists = result.unreadCount > 0;
                $.each(result.items, function (index, item) {
                    result.notifications.push(_appUserNotificationHelper.format(item));
                });

                var $li = $('#header_notification_bar');

                var template = $('#headerNotificationBarTemplate').html();
                Mustache.parse(template);
                var rendered = Mustache.render(template, result);

                $li.html(rendered);

                bindNotificationEvents();
            });
        }

        abp.event.on('abp.notifications.received', function (userNotification) {
            _appUserNotificationHelper.show(userNotification);
            loadNotifications();
        });

        abp.event.on('app.notifications.refresh', function () {
            loadNotifications();
        });

        abp.event.on('app.notifications.read', function (userNotificationId) {
            loadNotifications();
        });

        //Chat
        abp.event.on('app.chat.unreadMessageCountChanged', function (messageCount) {
            if (messageCount) {
                $('#UnreadChatMessageCount').removeClass('d-none');
            } else {
                $('#UnreadChatMessageCount').addClass('d-none');
            }

            $('#UnreadChatMessageCount').text(messageCount);
        });


        //Print Orders

        var printOrdersWithDeliveryInfoModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Orders/PrintOrdersWithDeliveryInfoModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Orders/_PrintOrdersWithDeliveryInfoModal.js',
            modalClass: 'PrintOrdersWithDeliveryInfoModal',
            modalSize: 'sm'
        });

        $('#PrintOrdersWithDeliveryInfoNavbarItem').click(function (e) {
            e.preventDefault();
            printOrdersWithDeliveryInfoModal.open();
        });

        //Payment Reconciliation Report

        var printPaymentReconciliationReportModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Orders/PrintPaymentReconciliationReportModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Orders/_PrintPaymentReconciliationReportModal.js',
            modalClass: 'PrintPaymentReconciliationReportModal',
            modalSize: 'sm'
        });

        $('#PrintPaymentReconciliationReportNavbarItem').click(function (e) {
            e.preventDefault();
            printPaymentReconciliationReportModal.open();
        });

        //Common 'Add Job' button

        var _createOrEditJobModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Orders/CreateOrEditJobModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Orders/_CreateOrEditJobModal.js',
            modalClass: 'CreateOrEditJobModal',
            modalSize: 'lg'
        });

        $('#CommonAddJobButton').click(function (e) {
            e.preventDefault();
            date = moment().format('L');
            _createOrEditJobModal.open({
                deliveryDate: date,
                shift: ''
            });
        });

        //Common 'Add Order' button

        $("#CommonAddOrderButton").click(function (e) {
            e.preventDefault();
            window.location = abp.appPath + 'app/Orders/Details/';
        });

        //Common 'Add Quote' button

        $("#CommonAddQuoteButton").click(function (e) {
            e.preventDefault();
            window.location = abp.appPath + 'app/Quotes/Details/';
        });

        //Common 'Add Truck' button

        var _createOrEditTruckModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Trucks/CreateOrEditModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Trucks/_CreateOrEditModal.js',
            modalClass: 'CreateOrEditTruckModal'
        });

        $("#CommonAddTruckButton").click(function (e) {
            e.preventDefault();
            _createOrEditTruckModal.open();
        });

        //Common 'Add Driver' button

        var _createOrEditDriverModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Drivers/CreateOrEditModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Drivers/_CreateOrEditModal.js',
            modalClass: 'CreateOrEditDriverModal',
        });

        $("#CommonAddDriverButton").click(function (e) {
            e.preventDefault();
            _createOrEditDriverModal.open();
        });

        //Common 'Add User' button

        var _createOrEditUserModal = new app.ModalManager({
            viewUrl: abp.appPath + 'App/Users/CreateOrEditModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/App/Views/Users/_CreateOrEditModal.js',
            modalClass: 'CreateOrEditUserModal'
        });

        $("#CommonAddUserButton").click(function (e) {
            e.preventDefault();
            _createOrEditUserModal.open();
        });

        function init() {
            loadNotifications();
            getRecentlyLinkedUsers();
            _appUserNotificationHelper.showPriorityNotifications();
        }

        init();
    });
})(jQuery);