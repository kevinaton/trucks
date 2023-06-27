import enTranslations from '../localization/en.json';

export const getText = (key) => {
    return enTranslations[key] || key; // Return the translated text or the key itself if not found
}