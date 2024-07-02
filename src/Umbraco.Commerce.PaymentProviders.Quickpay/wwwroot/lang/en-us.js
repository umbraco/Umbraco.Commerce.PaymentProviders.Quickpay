export default {
    paymentProviders: {
        'quickpayV10CheckoutLabel': 'Quickpay V10',
        'quickpayV10CheckoutDescription': 'Quickpay V10 payment provider for one time payments',
        'quickpayV10CheckoutSettingsContinueUrlLabel': 'Continue URL',
        'quickpayV10CheckoutSettingsContinueUrlDescription': 'The URL to continue to after this provider has done processing. eg: /continue/',
        'quickpayV10CheckoutSettingsCancelUrlLabel': 'Cancel URL',
        'quickpayV10CheckoutSettingsCancelUrlDescription': 'The URL to return to if the payment attempt is canceled. eg: /cart/',
        'quickpayV10CheckoutSettingsErrorUrlLabel': 'Error URL',
        'quickpayV10CheckoutSettingsErrorUrlDescription': 'The URL to return to if the payment attempt errors. eg: /error/',

        'quickpayV10CheckoutSettingsApiKeyLabel': 'API Key',
        'quickpayV10CheckoutSettingsApiKeyDescription': 'API Key from the Quickpay administration portal',

        'quickpayV10CheckoutSettingsPrivateKeyLabel': 'Private Key',
        'quickpayV10CheckoutSettingsPrivateKeyDescription': 'Private Key from the Quickpay administration portal',

        'quickpayV10CheckoutSettingsMerchantIdLabel': 'Merchant ID',
        'quickpayV10CheckoutSettingsMerchantIdDescription': 'Merchant ID supplied by Quickpay during registration',

        'quickpayV10CheckoutSettingsAgreemendIdLabel': 'Agreement ID',
        'quickpayV10CheckoutSettingsAgreemendIdDescription': 'Agreement ID supplied by Quickpay during registration',

        'quickpayV10CheckoutSettingsLangLabel': 'Language',
        'quickpayV10CheckoutSettingsLangDescription': 'The language of the payment portal to display',

        'quickpayV10CheckoutSettingsPaymentMethodsLabel': 'Accepted Payment Methods',
        'quickpayV10CheckoutSettingsPaymentMethodsDescription': 'A comma separated list of Payment Methods to accept. To use negation just put a "!" in front the those you do not wish to accept',

        'quickpayV10CheckoutSettingsAutoFeeLabel': 'Auto Fee',
        'quickpayV10CheckoutSettingsAutoFeeDescription': 'Flag indicating whether to automatically calculate and apply the fee from the acquirer',

        'quickpayV10CheckoutSettingsAutoCaptureLabel': 'Auto Capture',
        'quickpayV10CheckoutSettingsAutoCaptureDescription': 'Flag indicating whether to immediately capture the payment, or whether to just authorize the payment for later (manual) capture',

        'quickpayV10CheckoutSettingsFramedLabel': 'Framed',
        'quickpayV10CheckoutSettingsFramedDescription': 'Flag indicating whether to allow opening payment page in iframe',
    },
};