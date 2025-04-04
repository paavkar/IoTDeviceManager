import { useEffect, useState } from 'react'
import { Navigate, createBrowserRouter, RouterProvider } from 'react-router-dom';

import { SignIn } from './pages/login';
import { Register } from './pages/register';
import { UserLocalStorage } from './types';

function App() {
  const [isAuth, setIsAuth] = useState(false)

  function RedirectToAngular() {
    useEffect(() => {
      window.location.href = '/angular/';
    }, []);
  
    return null;
  }

  useEffect(() => {
    const userJson = localStorage.getItem('user');
    const currentTime = new Date();
    if (userJson) {
      try {
        const user: UserLocalStorage = JSON.parse(userJson);
        if (currentTime < new Date(user.data.tokenInfo.refreshTokenExpiresAt)) {
          setIsAuth(true);
          window.location.href = '/angular/';
        } else {
          localStorage.clear()
          setIsAuth(false);
        }
      } catch (error) {
        setIsAuth(false);
      }
    } else {
      setIsAuth(false);
    }
  }, [])

  const router = createBrowserRouter([
    {
        path: '/',
        element: isAuth ? <RedirectToAngular /> : <SignIn />,
    },
    {
        path: '/react',
        element: isAuth ? <RedirectToAngular /> : <SignIn />,
    },
    {
        path:'/login',
        element: isAuth ? <Navigate to="/" /> : <SignIn />
    },
    {
        path:'/register',
        element: isAuth ? <Navigate to="/" /> : <Register />
    },
    // {
    //     path: '/profile/:userName',
    //     element: <ProfilePage />
    // },
  ])

  return (
    <>
      <RouterProvider router={router} />
    </>
  )
}

export default App
