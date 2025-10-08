export interface IBoat {
	boatId: number;
	name: string;
	capacity: number;
	assignedUsersCount: number;
	type?: string;
	description?: string;
	location?: string;
	hourlyRate?: number;
	isActive?: boolean;
}