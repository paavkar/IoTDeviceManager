import { Component, OnInit } from '@angular/core';
import { MenuItem } from 'primeng/api';
import { Title } from '@angular/platform-browser';
import { Router, NavigationEnd, ActivatedRoute } from '@angular/router';
import { filter, map, mergeMap } from 'rxjs/operators';
import { Store } from '@ngrx/store';
import { inject } from '@angular/core';
import { HttpResponse } from '@angular/common/http';

import { User } from '../types';
import * as UserSelector from './state/user.selector';
import * as UserActions from './state/user.actions';
import { DataService } from './services/data.service';
import * as DevicesActions from './state/devices.actions';

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
    store.select(UserSelector.selectCurrentUser).subscribe(
      (user) => {
        this.user = user;
      }
    )
  }
  user: User | null = null;
  tile = 'IDM';
  year = new Date().getFullYear();
  mainMenuItems: MenuItem[] | undefined;
  userMenuItems: MenuItem[] | undefined;

  private dataService = inject(DataService)

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

    this.router.events.subscribe(event => {
      if (event instanceof NavigationEnd) {
        let currentTime = new Date();

        if (this.user == null) {
          this.dataService.fetchUser().subscribe(
            (response: HttpResponse<User>) => {
              if (response.ok && response.body) {
                this.store.dispatch(UserActions.loadUserSuccess({ user: response.body }))
                this.user = response.body
              }
            }
          )
        }

        if (this.user != null && currentTime > this.user?.tokenInfo.refreshTokenExpiresAt) {
            this.store.dispatch(UserActions.logout())
            this.store.dispatch(DevicesActions.unloadDevices())
            window.location.href = '/';
        }
        else if (currentTime > this.user!.tokenInfo.accessTokenExpiresAt
            && currentTime < this.user!.tokenInfo.refreshTokenExpiresAt) {
          this.dataService.refreshLogin().subscribe(
            (response: HttpResponse<User>) => {
              if (response.ok && response.body) {
                this.store.dispatch(UserActions.loadUserSuccess({ user: response.body }));
                this.user = response.body;
              }
              else {
                this.dataService.logout().subscribe(
                  (response: HttpResponse<string>) => {
                    if (response.ok) {
                      this.store.dispatch(UserActions.logout())
                      this.store.dispatch(DevicesActions.unloadDevices())
                      window.location.href = '/';
                    }
                  }
                )
              }
            }
          )
        }
      }
    })

    this.userMenuItems = [
      {
        label: this.user?.userName,
        icon: 'pi pi-user',
        items: [
          {
            label: 'Profile',
            icon: 'pi pi-user'
          },
          {
            label: 'Logout',
            icon: 'pi pi-sign-out',
            command: () => {
              this.dataService.logout();
              this.store.dispatch(UserActions.logout())
              this.store.dispatch(DevicesActions.unloadDevices())
              window.location.href = '/';
            }
          }
        ]
      }
    ]
  }

  toggleDarkMode() {
    const element = document.querySelector('html');
    element?.classList.toggle('idm-dark');
  }
}