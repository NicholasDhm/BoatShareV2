import { Injectable, Signal, signal } from '@angular/core';
import { BoatService } from '../../services/boat.service';
import { IBoat } from '../../models/boat';

export interface IBoatTestingViewModel {
	getBoatId: string;

	postBoatId: string;
	postName: string;
	postType: string;
	postCapacity: number;

	deleteBoatId: string;
}

@Injectable({
	providedIn: 'root'
})
export class BoatTestPageService {

	_viewModel = signal<IBoatTestingViewModel>({
		getBoatId: "1",

		postBoatId: "1",
		postName: 'name',
		postType: 'type',
		postCapacity: 0,
		
		deleteBoatId: "1",
	});

	constructor(
		private _boatService: BoatService,
	) {}

	get viewModel(): Signal<IBoatTestingViewModel> {
		return this._viewModel;
	}

	updateViewModel(value: string | number, type: keyof IBoatTestingViewModel): void {
		this._viewModel.set(Object.assign({}, this._viewModel(), { [type]: value }));
	}

	getAllBoats(): Promise<IBoat[]> {
		return this._boatService.getAllBoats().then(boats => {
			return boats;
		})
	}

	getBoatById(): Promise<IBoat> {
		return this._boatService.getBoatByBoatId(this._viewModel().getBoatId).then(boat => {
			return boat;
		})
	}

	postBoat(): Promise<IBoat> {
		const boatModel: IBoat = {
			boatId: this._viewModel().postBoatId,
			name: this._viewModel().postName,
			// type: this._viewModel().postType,
			capacity: this._viewModel().postCapacity,
			assignedUsersCount: 0,
		};

		return this._boatService.postBoat(boatModel).then(boat => {
			return boat;
		});
	}

	deleteBoatById(): void {
		this._boatService.deleteBoatById(this._viewModel().deleteBoatId);
	}

}