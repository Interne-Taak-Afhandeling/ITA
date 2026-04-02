export type User = {
  isLoggedIn: boolean;
  email: string;
  name: string;
  roles: string[];
  hasITASystemAccess: boolean;
  hasFunctioneelBeheerderAccess: boolean;
  hasOrganisatieCoordinatorAccess: boolean;
  hasTeamCoordinatorAccess: boolean;
  hasCoordinatorAccess: boolean;
};
