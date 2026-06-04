export type User = {
  isLoggedIn: boolean;
  email: string;
  name: string;
  objectregisterMedewerkerId: string;
  roles: string[];
  hasITASystemAccess: boolean;
  hasFunctioneelBeheerderAccess: boolean;
};
