export interface IUser {
	userId: string;
	name: string;
	email: string;
	role: 'Member' | 'Admin';
	standardQuota: number;
	substitutionQuota: number;
	contingencyQuota: number;
	boatId: string;
	passwordHash?: string;
}

export interface UserDTO {
	name: string;
	email: string;
	boatId: string;
	password: string;
}