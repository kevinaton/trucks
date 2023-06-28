import enTranslations from '../localization/en.json';

export const getText = (key, newValue) => {
    let msg = enTranslations[key] || key; // Return the translated text or the key itself if not found

    if (newValue) {
        msg = msg.replace('{0}', newValue);
    }

    return msg;
}