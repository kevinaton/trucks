const messageFormatters = {
    'DispatcherWeb.Infrastructure.Notifications.ImportCompletedNotificationData': (importCompletedNotification) => {
        return 'Import completed. Click to go to Import Results page.'
    },
    'Abp.Notifications.MessageNotificationData': (userNotification) => {
        return userNotification.data.message || userNotification.data.properties.Message
    }
}

export const getFormattedMessageFromUserNotification = (userNotification) => {  
    var formatter = messageFormatters[userNotification.data.type]
    if (!formatter) {
        console.warn('No message formatter defined for given data type: ' + userNotification.data.type)
        return '?'
    }

    if (typeof formatter !== 'function') {
        console.warn('Message formatter should be a function! It is invalid for data type: ' + userNotification.data.type)
        return '?'
    }

    return formatter(userNotification)
}

export const getUrl = (userNotification) => {
    switch(userNotification.notification.notificationName) {
        case 'App.NewUserRegistered':
            return '/App/users?filterText=' + userNotification.notification.data.properties.emailAddress
        case 'App.NewTenantRegistered':
            return '/App/tenants?filterText=' + userNotification.notification.data.properties.tenancyName
        case 'App.GdprDataPrepared':
            return '/File/DownloadBinaryFile?id=' + userNotification.notification.data.properties.binaryObjectId + '&contentType=application/zip&fileName=collectedData.zip'
        case 'App.ImportCompleted':
            return userNotification.notification.data.url
        default:
            return null
    }
}