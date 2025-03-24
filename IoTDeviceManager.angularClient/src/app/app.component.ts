import { Component, OnInit } from '@angular/core';
import { MenuItem } from 'primeng/api';
import { Title } from '@angular/platform-browser';
import { Router, NavigationEnd, ActivatedRoute } from '@angular/router';
import { filter, map, mergeMap } from 'rxjs/operators';
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs';

import { User } from '../types';
import * as UserSelector from './state/user.selector';
import * as UserActions from './state/user.actions';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  standalone: false,
  styleUrl: './app.component.css'
})

export class AppComponent implements OnInit {
  constructor(
    private titleService: Title,
    private router: Router,
    private activatedRoute: ActivatedRoute,
    private store: Store
  ) {
    this.user$ = store.select(UserSelector.selectCurrentUser)
  }
  user$: Observable<User | null>;
  tile = 'IDM';
  year = new Date().getFullYear();
  menuItems: MenuItem[] | undefined;

  ngOnInit(): void {
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd),
      map(() => {
        let route = this.activatedRoute.firstChild;
        while (route?.firstChild) {
          route = route.firstChild;
        }
        return route;
      }),
      mergeMap(route => route!.data)
    )
    .subscribe(data => {
      this.titleService.setTitle(data['title'] || 'IDM');
    });

    this.menuItems = [
      { label: 'Devices', routerLink: '/devices', icon: 'pi pi-home' }
    ]

    // if (this.user$ == null) {
    //   this.store.dispatch(UserActions.loadUser())
    // }
  }

  toggleDarkMode() {
    const element = document.querySelector('html');
    element?.classList.toggle('idm-dark');
  }
}
