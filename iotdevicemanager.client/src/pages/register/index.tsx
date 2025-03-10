import { useForm } from "react-hook-form"
import { NavLink } from "react-router-dom"
import { z } from "zod"
import { zodResolver } from "@hookform/resolvers/zod"
import { useState } from "react"
import { Button, Text, Field, Input, Title1 } from "@fluentui/react-components"

import { IdentityError, RegisteredUser } from "../../types"

const Schema = z.object({
    email: z.string().min(1, { message: "Email cannot be empty" }).email({ message: "Email must be of valid form" }),
    userName: z.string().min(1, {  message: "Username cannot be empty" }),
    password: z.string().min(8, { message: "Password must be at least 8 characters long" }),
    confirmPassword: z.string()
}).refine(data => data.password === data.confirmPassword, {
    message: "Passwords must match",
    path: ["confirmPassword"]
})

export const Register = () => {
    const { register, handleSubmit, formState: { errors, isSubmitting }, reset} = useForm<z.infer<typeof Schema>>({
        resolver: zodResolver(Schema),
        defaultValues: {
            email: "",
            userName: "",
            password: "",
            confirmPassword: ""
        }
    })
    const [httpErrors, setHttpErrors] = useState<IdentityError[]>([]);
    const [httpSuccess, setHttpSuccess] = useState("");

    async function onSubmit(values: z.infer<typeof Schema>) {
        var response = await fetch("/api/Auth/register", {
            method: "POST",
            body: JSON.stringify(values),
            headers: {
                "Content-Type": "application/json"
            }
        })

        if (!response.ok) {
            var error = await response.json()
            setHttpErrors(error.errors)

            setTimeout(() => setHttpErrors([]), 5000)
        }
        else {
            var user: RegisteredUser = await response.json()
            setHttpSuccess(`User created with email ${user.email} and username ${user.username}`)

            reset()
        }
    }

    return (
        <div style={{ display:' flex', alignItems:'center', flexDirection: 'column', width: '90vw', marginLeft: '5em' }}>
            <div style={{ marginBottom: '1em'}}>
                <Title1>Register</Title1>
            </div>
            <form onSubmit={handleSubmit(onSubmit)}>
                {httpErrors &&
                    httpErrors.map((error) => (
                        <Text style={{ color: 'red', width: '17em', height: '2em', borderRadius: '0.2em' }}>{error.description}</Text>
                    ))
                }

                {httpSuccess && 
                <p style={{ color: 'green', width: '17em', height: '2em', borderRadius: '0.2em' }}>{httpSuccess}</p>}

                <div style={{ display:'flex', flexDirection: 'column', marginTop: '2em' }}>
                    <Field label={"Email"} validationMessage={errors.email?.message}
                        style={{ marginBottom: "1em" }}>
                        <Input
                            {...register("email")}
                            type="email"
                            id="email"
                            placeholder="Email">
                        </Input>
                    </Field>

                    <Field label={"Username"} validationMessage={errors.userName?.message}
                        style={{ marginBottom: "1em" }}>
                        <Input
                            {...register("userName")}
                            id="userName"
                            placeholder="Username">
                        </Input>
                    </Field>

                    <Field label={"Password"} validationMessage={errors.password?.message}
                        style={{ marginBottom: "1em" }}>
                        <Input
                            {...register("password")}
                            type="password"
                            id="password"
                            placeholder="Password">
                        </Input>
                    </Field>

                    <Field label={"Confirm password"} validationMessage={errors.confirmPassword?.message}
                        style={{ marginBottom: "1em" }}>
                        <Input
                            {...register("confirmPassword")}
                            type="password"
                            id="confirmPassword"
                            placeholder="Confirm your password">
                        </Input>
                    </Field>

                    <Button type="submit" 
                        disabled={isSubmitting} 
                        style={{ marginTop: '1em', backgroundColor: 'green', height: '2.5em' }}>
                        Register
                    </Button>
                </div>

                <div>
                    <div style={{ display:'flex', flexDirection: 'column', marginTop: '1em' }}>
                        <Text>Already have an account?</Text>
                        <NavLink to="/login">Sign in here.</NavLink> 
                    </div>
                </div>
            </form>
        </div>
    )
}