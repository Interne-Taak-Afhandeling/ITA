// https://github.com/django/django/blob/4.2/django/core/validators.py#L174
export const EMAIL_PATTERN = new RegExp(
  "^" +
    // user_regex (without quoted string)
    "([-!#$%&'*+/=?^_`{}|~0-9A-Z]+(\\.[-!#$%&'*+/=?^_`{}|~0-9A-Z]+)*)" +
    "@" +
    // domain_regex (without literal_regex)
    "((?:[A-Z0-9](?:[A-Z0-9-]{0,61}[A-Z0-9])?\\.)+(?:[A-Z0-9-]{2,63}(?<!-)))" +
    "$",
  "i"
);
