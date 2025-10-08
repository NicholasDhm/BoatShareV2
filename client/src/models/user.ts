export interface IUser {
	userId: number;
	name: string;
	email: string;
	role: 'Member' | 'Admin';
	standardQuota: number;
	substitutionQuota: number;
	contingencyQuota: number;
	boatId: number;
	boatName?: string;
	passwordHash?: string;
}

export interface UserDTO {
	name: string;
	email: string;
	boatId: number;
	password: string;
}