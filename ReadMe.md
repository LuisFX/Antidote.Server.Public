Stack overflow pointing to github issue related to the token authorization failing under the hood when Header.Kid has no or empty values.

https://stackoverflow.com/questions/67663848/idx10503-signature-validation-failed-token-does-not-have-a-kid-keys-tried-s

'Well done. Error is thrown any time string.IsNullOrEmpty(jwtToken.Header.Kid); happens and keysAttempted.Length > 0, which is misleading' 
    - when you generate a value that is larger than the security size, it passes without issue. Use the following website to GENERATE GREATER THAN 256: https://jwtsecret.com/generate


