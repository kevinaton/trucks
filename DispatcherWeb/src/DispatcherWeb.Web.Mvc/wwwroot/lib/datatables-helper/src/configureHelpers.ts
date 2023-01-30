var abp = abp || {};
abp.helperConfiguration = abp.helperConfiguration || {}; //todo add the interface

abp.helperConfiguration.getCurrentLanguageLocale = abp.helperConfiguration.getCurrentLanguageLocale || function () {
    return 'en';
    //for abp:
    //return abp.localization.currentLanguage.name
};

abp.helperConfiguration.getIanaTimezoneId = abp.helperConfiguration.getIanaTimezoneId || function () {
    return 'America/Indianapolis';
    //for abp:
    //return abp.timing.timeZoneInfo.iana.timeZoneId;
};

abp.helperConfiguration.getDefaultCurrencySymbol = abp.helperConfiguration.getDefaultCurrencySymbol || function () {
    return '$';
    //for abp:
    //return abp.setting.get('App.General.CurrencySymbol');
};