import { useEffect, useState } from 'react'
import { Navigate, createBrowserRouter, RouterProvider } from 'react-router-dom';

import { SignIn } from './pages/login';
import { Register } from './pages/register';

function App() {
  const [isAuth, setIsAuth] = useState(false)

  function RedirectToAngular() {
    useEffect(() => {
      window.location.href = '/angular/';
    }, []);
  
    return null;
  }

  useEffect(() => {
    const fetchUserProfile = async () => {
      try {
        const response = await fetch('/api/Auth/me', {
          credentials: 'include',
        });
      
        if (response.ok) {
          //const userData = await response.json();
          setIsAuth(true)
          window.location.href = '/angular/';
        } else {
          //const error = await response.json();
          setIsAuth(false)
        }
      } catch (error) {
        console.error("Error fetching user profile:", error);
      }
    }

    fetchUserProfile()
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
